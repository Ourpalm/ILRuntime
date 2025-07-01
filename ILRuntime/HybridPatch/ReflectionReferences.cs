using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ILRuntime.Hybrid
{
    class ReflectionReferences
    {
        ModuleDefinition module;
        MethodReference readObjInfo, readObjByIdxInfo, readResultInfo, readResultByIdxInfo, retrieveObjInfo, pushObjInfo, pushParameterInfo;
        Dictionary<TypeReference, MethodReference> readObjCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> readObjIdxCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> readResultCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> readResultIdxCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> retrieveObjCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> pushObjCache = new Dictionary<TypeReference, MethodReference>();
        Dictionary<TypeReference, MethodReference> pushParameterCache = new Dictionary<TypeReference, MethodReference>();
        public ReflectionReferences(ModuleDefinition module)
        {
            this.module = module;

            PatchableAssemblyType = module.ImportReference(typeof(PatchableAssemblyAttribute));
            PatchableAssemblyCtor = module.ImportReference(typeof(PatchableAssemblyAttribute).GetConstructors()[0]);
            IPatchableTypeType = module.ImportReference(typeof(IPatchableType));
            AssemblyPatchType = module.ImportReference(typeof(AssemblyPatch));
            MethodInfo mi = typeof(AssemblyPatch).GetMethod(nameof(AssemblyPatch.ApplyPatch));
            PatchApplyPatchMethod = module.ImportReference(mi);
            mi = typeof(object).GetMethod(nameof(GetHashCode));
            ObjectGetHashCodeMethod = module.ImportReference(mi);
            AppDomainType = module.ImportReference(typeof(Runtime.Enviorment.AppDomain));
            BeginInvokeMethod = module.ImportReference(typeof(Runtime.Enviorment.AppDomain).GetMethod("BeginInvoke"));
            RegisterTypeStaticFieldAccessorMethod = module.ImportReference(typeof(Runtime.Enviorment.AppDomain).GetMethod(nameof(Runtime.Enviorment.AppDomain.RegisterTypeStaticFieldAccessor)));
            mi = typeof(Runtime.Enviorment.AppDomain).GetMethod(nameof(Runtime.Enviorment.AppDomain.GetType), new Type[] { typeof(Type) });
            AppDomainGetTypeMethod = module.ImportReference(mi);
            ILMethodType = module.ImportReference(typeof(ILMethod));
            ILTypeType = module.ImportReference(typeof(ILType));
            ITypeType = module.ImportReference(typeof(IType));
            ITypeArrayType = module.ImportReference(typeof(IType[]));
            KeyValuePairType = module.ImportReference(typeof(KeyValuePair<string, IType>));
            KeyValuePairCtor = module.ImportReference(typeof(KeyValuePair<string, IType>).GetConstructor(new Type[] { typeof(string), typeof(IType)}));
            KeyValuePairArrayType = module.ImportReference(typeof(KeyValuePair<string, IType>[]));
            PatchGetFieldDelegateCtor = module.ImportReference(typeof(PatchGetFieldDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            PatchSetFieldDelegateCtor = module.ImportReference(typeof(PatchSetFieldDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));

            mi = typeof(ILType).GetMethod(nameof(ILType.Instantiate), new Type[] { typeof(bool) });
            ILTypeInstantiateMethod = module.ImportReference(mi);
            mi = typeof(ILType).GetMethod(nameof(ILType.MakeGenericInstance));
            ILTypeMakeGenericInstanceMethod = module.ImportReference(mi);
            mi = typeof(ILType).GetMethod(nameof(ILType.GetMethodByGenericDefinition));
            ILTypeGetMethodByGenericDefinition = module.ImportReference(mi);
            mi = typeof(ILMethod).GetMethod(nameof(ILMethod.MakeGenericMethod));
            ILMethodMakeGenericInstanceMethod = module.ImportReference(mi);
            ILTypeInstanceType = module.ImportReference(typeof(ILTypeInstance));
            ILMethodArrayType = module.ImportReference(typeof(ILMethod).MakeArrayType());
            ILTypeArrayType = module.ImportReference(typeof(ILType).MakeArrayType());
            InvocationCtxType = module.ImportReference(typeof(InvocationContext));
            TypeArrayType = module.ImportReference(typeof(Type).MakeArrayType());
            TypeType = module.ImportReference(typeof(Type));

            GetTypeFromHandle = module.ImportReference(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));

            var methods = typeof(InvocationContext).GetMethods();

            mi = typeof(InvocationContext).GetMethod("Invoke");
            InvocationCtxInvokeMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.PushInteger), new Type[] { typeof(int) });
            PushInt32Method = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.PushInteger), new Type[] { typeof(long) });
            PushInt64Method = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.PushFloat), new Type[] { typeof(float) });
            PushFloatMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.PushDouble), new Type[] { typeof(double) });
            PushDoubleMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.PushBool), new Type[] { typeof(bool) });
            PushBoolMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("PushObject", new Type[] { typeof(object), typeof(bool) });
            PushObjectMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod("ReadInteger", new Type[] { typeof(int) });
            ReadInt32ByIndexMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.ReadLong), new Type[] { typeof(int) });
            ReadInt64ByIndexMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.ReadFloat), new Type[] { typeof(int) });
            ReadFloatByIndexMethod = module.ImportReference(mi);

            mi = typeof(InvocationContext).GetMethod(nameof(InvocationContext.ReadDouble), new Type[] { typeof(int) });
            ReadDoubleByIndexMethod = module.ImportReference(mi);

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
                        readObjInfo = module.ImportReference(method);
                    else
                        readObjByIdxInfo = module.ImportReference(method);
                }
                if (method.Name == "ReadResult" && method.IsGenericMethodDefinition)
                {
                    if (cnt == 0)
                        readResultInfo = module.ImportReference(method);
                    else if (method.GetParameters()[0].ParameterType == typeof(int))
                        readResultByIdxInfo = module.ImportReference(method);
                }
                if (method.Name == "ReadInteger" && !method.ContainsGenericParameters && cnt == 0)
                {
                    ReadInt32Method = module.ImportReference(method);
                }
                if (method.Name == "ReadLong" && !method.ContainsGenericParameters && cnt == 0)
                {
                    ReadInt64Method = module.ImportReference(method);
                }
                if (method.Name == "ReadFloat" && !method.ContainsGenericParameters && cnt == 0)
                {
                    ReadFloatMethod = module.ImportReference(method);
                }
                if (method.Name == "ReadDouble" && !method.ContainsGenericParameters && cnt == 0)
                {
                    ReadDoubleMethod = module.ImportReference(method);
                }
                if (method.Name == nameof(InvocationContext.PushParameter) && method.ContainsGenericParameters && cnt == 1)
                {
                    pushParameterInfo = module.ImportReference(method);
                }
            }

            mi = typeof(InvocationContext).GetMethod("Dispose");
            DisposeMethod = module.ImportReference(mi);

            StackObjectRefType = module.ImportReference(typeof(StackObject*));
            ILInterpreterType = module.ImportReference(typeof(ILIntepreter));
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.PushInt32), new Type[] { typeof(StackObject*), typeof(int) });
            InterpreterPushInt32Method = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.PushInt64), new Type[] { typeof(StackObject*), typeof(long) });
            InterpreterPushInt64Method = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.PushFloat), new Type[] { typeof(StackObject*), typeof(float) });
            InterpreterPushFloatMethod = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.PushDouble), new Type[] { typeof(StackObject*), typeof(double) });
            InterpreterPushDoubleMethod = module.ImportReference(mi);
            foreach( var i in typeof(ILIntepreter).GetMethods())
            {
                if(i.Name == nameof(ILIntepreter.PushObject) && i.ContainsGenericParameters)
                {
                    pushObjInfo = module.ImportReference(i);
                    break;
                }
            }
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.RetriveInt32), new Type[] { typeof(StackObject*), typeof(IList<object>) });
            InterpreterRetrieveInt32Method = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.RetriveInt64), new Type[] { typeof(StackObject*), typeof(IList<object>) });
            InterpreterRetrieveInt64Method = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.RetriveFloat), new Type[] { typeof(StackObject*), typeof(IList<object>) });
            InterpreterRetrieveFloatMethod = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.RetriveDouble), new Type[] { typeof(StackObject*), typeof(IList<object>) });
            InterpreterRetrieveDoubleMethod = module.ImportReference(mi);
            mi = typeof(ILIntepreter).GetMethod(nameof(ILIntepreter.RetrieveObject), new Type[] { typeof(StackObject*), typeof(IList<object>) });
            retrieveObjInfo = module.ImportReference(mi);
            ObjectIListType = module.ImportReference(typeof(IList<object>));
        }

        public TypeReference PatchableAssemblyType { get; private set; }
        public TypeReference IPatchableTypeType { get; private set; }
        public MethodReference PatchableAssemblyCtor { get; private set; }
        public TypeReference AssemblyPatchType { get; private set; }
        public MethodReference PatchApplyPatchMethod { get; private set; }

        public MethodReference ObjectGetHashCodeMethod { get; private set; }

        public TypeReference AppDomainType { get; private set; }
        public TypeReference ILMethodType { get; private set; }
        public TypeReference ILTypeType { get; private set; }
        public MethodReference ILTypeInstantiateMethod { get; private set; }
        public TypeReference ILTypeInstanceType { get; private set; }
        public TypeReference ILMethodArrayType { get; private set; }
        public TypeReference ILTypeArrayType { get; private set; }
        public TypeReference ITypeArrayType { get; private set; }
        public TypeReference ITypeType { get; private set; }

        public TypeReference TypeArrayType { get; private set; }
        public TypeReference TypeType { get; private set; }

        public TypeReference KeyValuePairType { get; private set; }
        public MethodReference KeyValuePairCtor { get; private set; }
        public TypeReference KeyValuePairArrayType { get; private set; }

        public MethodReference PatchGetFieldDelegateCtor { get; private set; }
        public MethodReference PatchSetFieldDelegateCtor { get; private set; }


        public MethodReference ILTypeMakeGenericInstanceMethod { get;private set; }
        public MethodReference ILTypeGetMethodByGenericDefinition { get; private set; }

        public MethodReference ILMethodMakeGenericInstanceMethod { get; private set; }

        public MethodReference AppDomainGetTypeMethod { get; private set; }

        public MethodReference BeginInvokeMethod { get; private set; }
        public MethodReference RegisterTypeStaticFieldAccessorMethod { get; private set; }
        public TypeReference InvocationCtxType { get; private set; }
        public MethodReference InvocationCtxInvokeMethod { get; private set; }

        public MethodReference PushBoolMethod { get; private set; }
        public MethodReference PushInt32Method { get; private set; }
        public MethodReference PushInt64Method { get; private set; }
        public MethodReference PushFloatMethod { get; private set; }
        public MethodReference PushDoubleMethod { get; private set; }
        public MethodReference PushObjectMethod { get; private set; }
        public MethodReference PushReferenceMethod { get; private set; }

        public MethodReference ReadInt32Method { get; private set; }
        public MethodReference ReadInt32ByIndexMethod { get; private set; }
        public MethodReference ReadInt64Method { get; private set; }
        public MethodReference ReadInt64ByIndexMethod { get; private set; }
        public MethodReference ReadFloatMethod { get; private set; }
        public MethodReference ReadFloatByIndexMethod { get; private set; }
        public MethodReference ReadDoubleMethod { get; private set; }
        public MethodReference ReadDoubleByIndexMethod { get; private set; }
        public MethodReference DisposeMethod { get; private set; }

        public TypeReference StackObjectRefType { get; private set; }
        public TypeReference ILInterpreterType {  get; private set; }
        public TypeReference ObjectIListType { get; private set; }

        public MethodReference InterpreterPushInt32Method { get; private set; }
        public MethodReference InterpreterPushInt64Method { get; private set; }
        public MethodReference InterpreterPushFloatMethod { get; private set; }
        public MethodReference InterpreterPushDoubleMethod { get; private set; }
        public MethodReference InterpreterRetrieveInt32Method { get; private set; }
        public MethodReference InterpreterRetrieveInt64Method { get; private set; }
        public MethodReference InterpreterRetrieveFloatMethod { get; private set; }
        public MethodReference InterpreterRetrieveDoubleMethod { get; private set; }

        public MethodReference GetTypeFromHandle { get; private set; }

        public MethodReference GetReadObjectMethod(TypeReference type)
        {
            if (readObjCache.TryGetValue(type, out var res))
                return res;
            res = readObjInfo.MakeGenericInstanceMethod(type);
            readObjCache[type] = res;
            return res;
        }

        public MethodReference GetReadObjectByIndexMethod(TypeReference type)
        {
            if (readObjIdxCache.TryGetValue(type, out var res))
                return res;
            res = readObjByIdxInfo.MakeGenericInstanceMethod(type);
            readObjIdxCache[type] = res;
            return res;
        }

        public MethodReference GetReadResultMethod(TypeReference type)
        {
            if (readResultCache.TryGetValue(type, out var res))
                return res;
            res = readResultInfo.MakeGenericInstanceMethod(type);
            readResultCache[type] = res;
            return res;
        }

        public MethodReference GetReadResultByIndexMethod(TypeReference type)
        {
            if (readObjIdxCache.TryGetValue(type, out var res))
                return res;
            res = readResultByIdxInfo.MakeGenericInstanceMethod(type);
            readObjIdxCache[type] = res;
            return res;
        }

        public MethodReference GetRetrieveObjectMethod(TypeReference type)
        {
            if (retrieveObjCache.TryGetValue(type, out var res))
                return res;
            res = retrieveObjInfo.MakeGenericInstanceMethod(type);
            retrieveObjCache[type] = res;
            return res;
        }

        public MethodReference GetPushObjectMethod(TypeReference type)
        {
            if (pushObjCache.TryGetValue(type, out var res))
                return res;
            res = pushObjInfo.MakeGenericInstanceMethod(type);
            pushObjCache[type] = res;
            return res;
        }

        public MethodReference GetPushParameterMethod(TypeReference type)
        {
            if (pushParameterCache.TryGetValue(type, out var res))
                return res;
            res = pushParameterInfo.MakeGenericInstanceMethod(type);
            pushParameterCache[type] = res;
            return res;
        }
    }
}
