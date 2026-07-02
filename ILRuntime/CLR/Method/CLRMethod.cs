using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.CLR.Method
{
    public sealed class CLRMethod : IMethod
    {
        MethodInfo def;
        ConstructorInfo cDef;
        List<IType> parameters;
        ParameterInfo[] parametersCLR;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        CLRType declaringType;
        bool isConstructor;
        CLRRedirectionDelegate redirect;
        CLRRedirectionDelegateNeo redirectNeo;
        IType[] genericArguments;
        Type[] genericArgumentsCLR;
        object[] invocationParam;
        bool isDelegateInvoke, isDelegateDynamicInvoke;
        int hashCode = -1;
        static int instance_id = 0x20000000;

        public IType DeclearingType
        {
            get
            {
                return declaringType;
            }
        }
        public string Name
        {
            get
            {
                return def != null ? def.Name : cDef.Name;
            }
        }
        public bool HasThis
        {
            get
            {
                return isConstructor ? !cDef.IsStatic : !def.IsStatic;
            }
        }

        int _genericParameterCount = -1;
        public int GenericParameterCount
        {
            get
            {
                if (_genericParameterCount == -1)
                {
                    if (def.ContainsGenericParameters && def.IsGenericMethodDefinition)
                        _genericParameterCount = def.GetGenericArguments().Length;
                    else
                        _genericParameterCount = 0;
                }
                return _genericParameterCount;
            }
        }

        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public bool IsDelegateInvoke
        {
            get
            {
                return isDelegateInvoke;
            }
        }

        public bool IsDelegateDynamicInvoke
        {
            get
            {
                return isDelegateDynamicInvoke;
            }
        }

        public bool IsStatic
        {
            get
            {
                if (cDef != null)
                    return cDef.IsStatic;
                else
                    return def.IsStatic;
            }
        }

        bool TryGetRedirection<T>(Dictionary<MethodBase, T> map, out T redirect)
        {
            redirect = default;
            if (def != null)
            {
                if (def.IsGenericMethod && !def.IsGenericMethodDefinition)
                {
                    if (!map.TryGetValue(def.GetGenericMethodDefinition(), out redirect))
                        map.TryGetValue(def, out redirect);
                }
                else
                    map.TryGetValue(def, out redirect);
                return redirect != null;
            }
            else if (cDef != null)
            {
                map.TryGetValue(cDef, out redirect);
                return redirect != null;
            }
            return false;
        }

        public CLRRedirectionDelegate Redirection
        {
            get
            {
                if (redirect == null)
                {
                    TryGetRedirection(appdomain.RedirectMap, out redirect);
                }
                return redirect;
            }
        }

        public CLRRedirectionDelegateNeo RedirectionNeo
        {
            get
            {
                if (redirectNeo == null)
                {
                    TryGetRedirection(appdomain.RedirectMapNeo, out redirectNeo);
                }
                return redirectNeo;
            }
        }

        public MethodInfo MethodInfo { get { return def; } }

        public ConstructorInfo ConstructorInfo { get { return cDef; } }

        public IType[] GenericArguments { get { return genericArguments; } }

        public Type[] GenericArgumentsCLR
        {
            get
            {
                if (genericArgumentsCLR == null)
                {
                    if (cDef != null)
                        genericArgumentsCLR = cDef.GetGenericArguments();
                    else
                        genericArgumentsCLR = def.GetGenericArguments();
                }
                return genericArgumentsCLR;
            }
        }

        internal CLRMethod(MethodInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.def = def;
            declaringType = type;
            this.appdomain = domain;
            if (!def.ReturnType.ContainsGenericParameters)
            {
                ReturnType = domain.GetType(def.ReturnType.FullName);
                if (ReturnType == null)
                {
                    ReturnType = domain.GetType(def.ReturnType.AssemblyQualifiedName);
                }
            }
            if (type.IsDelegate)
            {
                if (def.Name == "Invoke")
                    isDelegateInvoke = true;
                if (def.Name == "DynamicInvoke")
                {
                    isDelegateInvoke = true;
                    isDelegateDynamicInvoke = true;
                }

            }
            isConstructor = false;
        }
        internal CLRMethod(ConstructorInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.cDef = def;
            declaringType = type;
            this.appdomain = domain;
            if (!def.ContainsGenericParameters)
            {
                ReturnType = type;
            }
            isConstructor = true;
        }

        public int ParameterCount
        {
            get
            {
                return Parameters.Count;
            }
        }


        public List<IType> Parameters
        {
            get
            {
                if (parameters == null)
                {
                    InitParameters();
                }
                return parameters;
            }
        }

        public ParameterInfo[] ParametersCLR
        {
            get
            {
                if (parametersCLR == null)
                {
                    if (cDef != null)
                        parametersCLR = cDef.GetParameters();
                    else
                        parametersCLR = def.GetParameters();
                }
                return parametersCLR;
            }
        }

        public IType ReturnType
        {
            get;
            private set;
        }

        string signatureString;
        public string SignatureString
        {
            get
            {
                if (signatureString == null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(Name);
                    sb.Append('|');
                    sb.Append(GenericParameterCount);
                    sb.Append('(');
                    var ps = Parameters;
                    if (ps != null)
                    {
                        for (int i = 0; i < ps.Count; i++)
                        {
                            if (i > 0)
                                sb.Append(',');
                            sb.Append(ps[i] != null ? ps[i].FullName : string.Empty);
                        }
                    }
                    sb.Append(")->");
                    sb.Append(ReturnType != null ? ReturnType.FullName : string.Empty);
                    signatureString = sb.ToString();
                }
                return signatureString;
            }
        }

        public bool IsConstructor
        {
            get
            {
                return cDef != null;
            }
        }

        void InitParameters()
        {
            parameters = new List<IType>();
            foreach (var i in ParametersCLR)
            {
                IType type = appdomain.GetType(i.ParameterType.FullName);
                if (type == null)
                    type = appdomain.GetType(i.ParameterType.AssemblyQualifiedName);
                if (i.ParameterType.IsGenericTypeDefinition)
                {
                    if (type == null)
                        type = appdomain.GetType(i.ParameterType.GetGenericTypeDefinition().FullName);
                    if (type == null)
                        type = appdomain.GetType(i.ParameterType.GetGenericTypeDefinition().AssemblyQualifiedName);
                }
                if (i.ParameterType.ContainsGenericParameters)
                {
                    var t = i.ParameterType;
                    if (t.HasElementType)
                        t = i.ParameterType.GetElementType();
                    else if (t.GetGenericArguments().Length > 0)
                    {
                        t = t.GetGenericArguments()[0];
                    }
                    type = new ILGenericParameterType(t.Name);
                }
                if (type == null)
                    throw new TypeLoadException();
                parameters.Add(type);
            }
        }

        unsafe StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

#if ENABLE_NEO_MODE
        public unsafe object Invoke(byte* targetBase, AutoList mStack, bool isNewObj = false)
        {
            if (parameters == null)
            {
                InitParameters();
            }
            int paramCount = ParameterCount;
            if (invocationParam == null)
                invocationParam = new object[paramCount];
            object[] param = invocationParam;

            int curPrim = 0;
            object instance = null;

            if (isNewObj)
            {
                curPrim += 4; // Skip retRefBase
            }
            else if (HasThis)
            {
                int thisIdx = *(int*)(targetBase + curPrim);
                instance = mStack[thisIdx];
                curPrim += 4;
            }

            for (int i = 0; i < paramCount; i++)
            {
                var pt = Parameters[i];
                Type t = pt.TypeForCLR;

                if (pt is CLRType clrType && clrType.IsValueType && !clrType.TypeForCLR.IsPrimitive && !clrType.TypeForCLR.IsEnum)
                {
                    throw new NotImplementedException("CLR value type reflection fallback: Step 13");
                }

                if (pt is ILType || !t.IsPrimitive && !t.IsEnum)
                {
                    int idx = *(int*)(targetBase + curPrim);
                    param[i] = mStack[idx];
                    curPrim += 4;
                }
                else
                {
                    if (t == typeof(int) || t.IsEnum) { param[i] = ILIntepreter.ReadNeoInt32(targetBase, ref curPrim); }
                    else if (t == typeof(long)) { param[i] = ILIntepreter.ReadNeoInt64(targetBase, ref curPrim); }
                    else if (t == typeof(float)) { param[i] = ILIntepreter.ReadNeoFloat(targetBase, ref curPrim); }
                    else if (t == typeof(double)) { param[i] = ILIntepreter.ReadNeoDouble(targetBase, ref curPrim); }
                    else if (t == typeof(bool)) { param[i] = ILIntepreter.ReadNeoBoolean(targetBase, ref curPrim); }
                    else if (t == typeof(byte)) { param[i] = ILIntepreter.ReadNeoUInt8(targetBase, ref curPrim); }
                    else if (t == typeof(sbyte)) { param[i] = ILIntepreter.ReadNeoInt8(targetBase, ref curPrim); }
                    else if (t == typeof(short)) { param[i] = ILIntepreter.ReadNeoInt16(targetBase, ref curPrim); }
                    else if (t == typeof(ushort)) { param[i] = ILIntepreter.ReadNeoUInt16(targetBase, ref curPrim); }
                    else if (t == typeof(uint)) { param[i] = ILIntepreter.ReadNeoUInt32(targetBase, ref curPrim); }
                    else if (t == typeof(ulong)) { param[i] = ILIntepreter.ReadNeoUInt64(targetBase, ref curPrim); }
                    else if (t == typeof(char)) { param[i] = ILIntepreter.ReadNeoChar(targetBase, ref curPrim); }
                }
            }

            object res = null;
            if (isConstructor)
            {
                if (!isNewObj)
                {
                    if (!cDef.IsStatic)
                    {
                        if (instance == null)
                            throw new NullReferenceException();
                        if (instance is CrossBindingAdaptorType && paramCount == 0)
                            return null;
                        cDef.Invoke(instance, param);
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    res = cDef.Invoke(param);
                }
            }
            else
            {
                if (!def.IsStatic)
                {
                    if (!(instance is Reflection.ILRuntimeWrapperType))
                        instance = declaringType.TypeForCLR.CheckCLRTypes(instance);
                    if (instance == null)
                        throw new NullReferenceException();
                }
                res = def.Invoke(instance, param);
            }

            Array.Clear(invocationParam, 0, invocationParam.Length);
            return res;
        }
#endif

        public unsafe object Invoke(Runtime.Intepreter.ILIntepreter intepreter, StackObject* esp, AutoList mStack, bool isNewObj = false)
        {
            if (parameters == null)
            {
                InitParameters();
            }
            int paramCount = ParameterCount;
            if (invocationParam == null)
                invocationParam = new object[paramCount];
            object[] param = invocationParam;
            for (int i = paramCount; i >= 1; i--)
            {
                var p = Minus(esp, i);
                var pt = this.ParametersCLR[paramCount - i].ParameterType;
                var obj = pt.CheckCLRTypes(StackObject.ToObject(p, appdomain, mStack));
                obj = ILIntepreter.CheckAndCloneValueType(obj, appdomain);
                param[paramCount - i] = obj;
            }

            if (isConstructor)
            {
                if (!isNewObj)
                {
                    if (!cDef.IsStatic)
                    {
                        object instance = declaringType.TypeForCLR.CheckCLRTypes(StackObject.ToObject((Minus(esp, paramCount + 1)), appdomain, mStack));
                        if (instance == null)
                            throw new NullReferenceException();
                        if (instance is CrossBindingAdaptorType && paramCount == 0)//It makes no sense to call the Adaptor's default constructor
                            return null;
                        cDef.Invoke(instance, param);
                        Array.Clear(invocationParam, 0, invocationParam.Length);
                        return null;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    var res = cDef.Invoke(param);
                    FixReference(paramCount, esp, param, mStack, null, false);
                    Array.Clear(invocationParam, 0, invocationParam.Length);
                    return res;
                }

            }
            else
            {
                object instance = null;

                if (!def.IsStatic)
                {
                    instance = StackObject.ToObject((Minus(esp, paramCount + 1)), appdomain, mStack);
                    if (!(instance is Reflection.ILRuntimeWrapperType))
                        instance = declaringType.TypeForCLR.CheckCLRTypes(instance);
                    //if (declaringType.IsValueType)
                    //    instance = ILIntepreter.CheckAndCloneValueType(instance, appdomain);
                    if (instance == null)
                        throw new NullReferenceException();
                }
                object res = null;
                /*if (redirect != null)
                    res = redirect(new ILContext(appdomain, intepreter, esp, mStack, this), instance, param, genericArguments);
                else*/
                {
                    res = def.Invoke(instance, param);
                }

                FixReference(paramCount, esp, param, mStack, instance, !def.IsStatic);
                Array.Clear(invocationParam, 0, invocationParam.Length);
                return res;
            }
        }

        unsafe void FixReference(int paramCount, StackObject* esp, object[] param, AutoList mStack, object instance, bool hasThis)
        {
            var cnt = hasThis ? paramCount + 1 : paramCount;
            for (int i = cnt; i >= 1; i--)
            {
                var p = Minus(esp, i);
                var val = i <= paramCount ? param[paramCount - i] : instance;
                switch (p->ObjectType)
                {
                    case ObjectTypes.StackObjectReference:
                        {
                            var addr = *(long*)&p->Value;
                            var dst = (StackObject*)addr;
                            if (dst->ObjectType >= ObjectTypes.Object)
                            {
                                var obj = val;
                                if (obj is CrossBindingAdaptorType)
                                    obj = ((CrossBindingAdaptorType)obj).ILInstance;
                                mStack[dst->Value] = obj;
                            }
                            else
                            {
                                ILIntepreter.UnboxObject(dst, val, mStack, appdomain);
                            }
                        }
                        break;
                    case ObjectTypes.FieldReference:
                        {
                            var obj = mStack[p->Value];
                            if (obj is ILTypeInstance)
                            {
                                ((ILTypeInstance)obj)[p->ValueLow] = val;
                            }
                            else
                            {
                                var t = appdomain.GetType(obj.GetType()) as CLRType;
                                t.GetField(p->ValueLow).SetValue(obj, val);
                            }
                        }
                        break;
                    case ObjectTypes.StaticFieldReference:
                        {
                            var t = appdomain.GetType(p->Value);
                            if (t is ILType)
                            {
                                ((ILType)t).StaticInstance[p->ValueLow] = val;
                            }
                            else
                            {
                                ((CLRType)t).SetStaticFieldValue(p->ValueLow, val);
                            }
                        }
                        break;
                    case ObjectTypes.ArrayReference:
                        {
                            var arr = mStack[p->Value] as Array;
                            arr.SetValue(val, p->ValueLow);
                        }
                        break;
                }
            }
        }

        public IMethod MakeGenericMethod(IType[] genericArguments)
        {
            Type[] p = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                p[i] = genericArguments[i].TypeForCLR;
            }

            MethodInfo t = null;
#if UNITY_EDITOR || (DEBUG && !DISABLE_ILRUNTIME_DEBUG)
            try
            {
#endif
                t = def.MakeGenericMethod(p);
#if UNITY_EDITOR || (DEBUG && !DISABLE_ILRUNTIME_DEBUG)
            }
            catch (Exception e)
            {
                string argString = "";
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    argString += genericArguments[i].TypeForCLR.FullName + ", ";
                }

                argString = argString.Substring(0, argString.Length - 2);
                throw new Exception(string.Format("MakeGenericMethod failed : {0}.{1}<{2}>", def.DeclaringType.FullName, def.Name, argString));
            }
#endif
            var res = new CLRMethod(t, declaringType, appdomain);
            res.genericArguments = genericArguments;
            return res;
        }

        public override string ToString()
        {
            if (def != null)
                return def.ToString();
            else
                return cDef.ToString();
        }

        public override int GetHashCode()
        {
            if (hashCode == -1)
                hashCode = System.Threading.Interlocked.Add(ref instance_id, 1);
            return hashCode;
        }


        bool? isExtend;
        public bool IsExtend
        {
            get
            {
                if (isExtend == null)
                {
                    isExtend = this.IsExtendMethod();
                }
                return isExtend.Value;
            }
        }
    }
}
