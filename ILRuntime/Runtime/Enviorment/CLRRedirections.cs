using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    static class CLRRedirections
    {
        public static object CreateInstance(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)
                {
                    return ((ILType)t).Instantiate();
                }
                else
                    throw new NotImplementedException();
            }
            else
                throw new EntryPointNotFoundException();
        }
        public unsafe static object InitializeArray(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            object array = param[0];
            byte[] data = param[1] as byte[];
            if (data == null)
                return null;
            fixed (byte* p = data)
            {
                if (array is int[])
                {
                    int[] arr = array as int[];
                    int* ptr = (int*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is byte[])
                {
                    byte[] arr = array as byte[];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = p[i];
                    }
                }
                else if (array is sbyte[])
                {
                    sbyte[] arr = array as sbyte[];
                    sbyte* ptr = (sbyte*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is short[])
                {
                    short[] arr = array as short[];
                    short* ptr = (short*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is ushort[])
                {
                    ushort[] arr = array as ushort[];
                    ushort* ptr = (ushort*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is char[])
                {
                    char[] arr = array as char[];
                    char* ptr = (char*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is uint[])
                {
                    uint[] arr = array as uint[];
                    uint* ptr = (uint*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is Int64[])
                {
                    long[] arr = array as long[];
                    long* ptr = (long*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is UInt64[])
                {
                    ulong[] arr = array as ulong[];
                    ulong* ptr = (ulong*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is float[])
                {
                    float[] arr = array as float[];
                    float* ptr = (float*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is double[])
                {
                    double[] arr = array as double[];
                    double* ptr = (double*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is bool[])
                {
                    bool[] arr = array as bool[];
                    bool* ptr = (bool*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else
                {
                    throw new NotImplementedException("array=" + array.GetType());
                }
            }

            return null;
        }

        public unsafe static object DelegateCombine(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            //Don't ask me why not esp -2, unity won't return the right result
            var dele1 = (esp - 1 - 1)->ToObject(domain, mStack);
            var dele2 = (esp - 1)->ToObject(domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            var dele = ((IDelegateAdapter)dele1);
                            //This means it's the default delegate which should be singleton to support == operator
                            if (dele.Next == null)
                            {
                                dele = dele.Instantiate(domain, dele.Instance, dele.Method);
                            }
                            dele.Combine((IDelegateAdapter)dele2);
                            return dele;
                        }
                        else
                        {
                            ((IDelegateAdapter)dele1).Combine((Delegate)dele2);
                            return dele1;
                        }
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return Delegate.Combine((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType()));
                        else
                            return Delegate.Combine((Delegate)dele1, (Delegate)dele2);
                    }
                }
                else
                    return dele1;
            }
            else
                return dele2;
        }

        public unsafe static object DelegateRemove(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = (esp - 1 - 1)->ToObject(domain, mStack);
            var dele2 = (esp - 1)->ToObject(domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            if (dele1 == dele2)
                                return ((IDelegateAdapter)dele1).Next;
                            else
                                ((IDelegateAdapter)dele1).Remove((IDelegateAdapter)dele2);
                        }
                        else
                            ((IDelegateAdapter)dele1).Remove((Delegate)dele2);
                        return dele1;
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return Delegate.Remove((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType()));
                        else
                            return Delegate.Remove((Delegate)dele1, (Delegate)dele2);
                    }
                }
                else
                    return dele1;
            }
            else
                return null;
        }

        public unsafe static object DelegateEqulity(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            //op_Equality,op_Inequality
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = (esp - 1 - 1)->ToObject(domain, mStack);
            var dele2 = (esp - 1)->ToObject(domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            return ((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            return ((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            return (Delegate)dele1 == ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        }
                        else
                            return (Delegate)dele1 == (Delegate)dele2;
                    }
                }
                else
                    return dele1 == null;
            }
            else
                return dele2 == null;
        }

        public unsafe static object DelegateInequlity(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            //op_Equality,op_Inequality
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = (esp - 1 - 1)->ToObject(domain, mStack);
            var dele2 = (esp - 1)->ToObject(domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            return !((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            return !((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return (Delegate)dele1 != ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        else
                            return (Delegate)dele1 != (Delegate)dele2;
                    }
                }
                else
                    return dele1 != null;
            }
            else
                return dele2 != null;
        }

        public static object GetTypeFromHandle(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            return param[0];
        }
    }
}
