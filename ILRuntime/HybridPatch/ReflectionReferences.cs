using ILRuntime.CLR.Method;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ILRuntime.Hybrid
{
    class ReflectionReferences
    {
        ModuleDefinition module;
        MethodInfo readObjInfo, readObjByIdxInfo;
        Dictionary<Type, MethodReference> readObjCache = new Dictionary<Type, MethodReference>();
        Dictionary<Type, MethodReference> readObjIdxCache = new Dictionary<Type, MethodReference>();
        public ReflectionReferences(ModuleDefinition module)
        {
            this.module = module;

            AppDomainType = module.ImportReference(typeof(Runtime.Enviorment.AppDomain));
            BeginInvokeMethod = module.ImportReference(typeof(Runtime.Enviorment.AppDomain).GetMethod("BeginInvoke"));
            ILMethodType = module.ImportReference(typeof(ILMethod));
            InvocationCtxType = module.ImportReference(typeof(InvocationContext));
            var methods = typeof(InvocationContext).GetMethods();

            MethodInfo mi = typeof(InvocationContext).GetMethod("PushBool");
            PushBoolMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("Invoke");
            InvocationCtxInvokeMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushInteger", new Type[] { typeof(int) });
            PushInt32Method = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushBool", new Type[] { typeof(bool) });
            PushBoolMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushObject", new Type[] { typeof(object), typeof(bool) });
            PushObjectMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("ReadInteger", new Type[] { typeof(int) });
            ReadInt32ByIndexMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushReference", new Type[] { typeof(int) });
            PushReferenceMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushObject", new Type[] { typeof(object), typeof(bool) });
            PushObjectMethod = module.ImportReference(mi);

            foreach (var method in methods)
            {
                int cnt = method.GetParameters().Length;
                if (method.Name == "ReadObject" && method.IsGenericMethodDefinition)
                {
                    if (cnt == 0)
                        readObjInfo = method;
                    else
                        readObjByIdxInfo = method;
                }
                if(method.Name == "ReadInteger" && !method.ContainsGenericParameters && cnt == 0)
                {
                    ReadInt32Method = module.ImportReference(method);
                }
            }

            mi = typeof(InvocationContext).GetMethod("Dispose");
            DisposeMethod = module.ImportReference(mi);
            
        }

        public TypeReference AppDomainType { get; private set; }
        public TypeReference ILMethodType { get; private set; }
        public MethodReference BeginInvokeMethod { get; private set; }
        public TypeReference InvocationCtxType { get; private set; }
        public MethodReference InvocationCtxInvokeMethod { get; private set; }

        public MethodReference PushBoolMethod { get; private set; }
        public MethodReference PushInt32Method { get; private set; }
        public MethodReference PushObjectMethod { get; private set; }
        public MethodReference PushReferenceMethod { get; private set; }

        public MethodReference ReadInt32Method { get; private set; }
        public MethodReference ReadInt32ByIndexMethod { get; private set; } 
        public MethodReference DisposeMethod { get; private set; }

        public MethodReference GetReadObjectMethod(Type type)
        {
            if (readObjCache.TryGetValue(type, out var res))
                return res;
            var mi = readObjInfo.MakeGenericMethod(type);
            res = module.ImportReference(mi);
            readObjCache[type] = res;
            return res;
        }

        public MethodReference GetReadObjectByIndexMethod(Type type)
        {
            if (readObjIdxCache.TryGetValue(type, out var res))
                return res;
            var mi = readObjByIdxInfo.MakeGenericMethod(type);
            res = module.ImportReference(mi);
            readObjIdxCache[type] = res;
            return res;
        }
    }
}
