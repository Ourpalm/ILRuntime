using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Reflection;
using Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Enviorment
{
    public class AppDomain
    {
        Queue<ILIntepreter> freeIntepreters = new Queue<ILIntepreter>();
        Dictionary<int, ILIntepreter> intepreters = new Dictionary<int, ILIntepreter>();
        Dictionary<Type, CrossBindingAdaptor> crossAdaptors = new Dictionary<Type, CrossBindingAdaptor>();
        Dictionary<string, IType> mapType = new Dictionary<string, IType>();
        Dictionary<Type, IType> clrTypeMapping = new Dictionary<Type, IType>();
        Dictionary<int, IType> mapTypeToken = new Dictionary<int, IType>();
        Dictionary<int, IMethod> mapMethod = new Dictionary<int, IMethod>();
        Dictionary<int, string> mapString = new Dictionary<int, string>();
        Dictionary<System.Reflection.MethodInfo, Func<ILContext, object, object[], IType[], object>> redirectMap = new Dictionary<System.Reflection.MethodInfo, Func<ILContext, object, object[], IType[], object>>();
        IType voidType, intType, longType, boolType, floatType, doubleType, objectType;
        DelegateManager dMgr;
        Assembly[] loadedAssemblies;
        Dictionary<string, byte[]> references = new Dictionary<string, byte[]>();
        DebugService debugService;
        public AppDomain()
        {
            loadedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var mi = typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray");
            RegisterCLRMethodRedirection(mi, CLRRedirections.InitializeArray);
            foreach (var i in typeof(System.Activator).GetMethods())
            {
                if (i.Name == "CreateInstance" && i.IsGenericMethodDefinition)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.CreateInstance);
                }
            }
            foreach (var i in typeof(System.Delegate).GetMethods())
            {
                if (i.Name == "Combine" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateCombine);
                }
                if(i.Name == "Remove")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateRemove);
                }
                if(i.Name == "op_Equality")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateEqulity);
                }
                if(i.Name == "op_Inequality")
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.DelegateInequlity);
                }
            }
            foreach(var i in typeof(MethodBase).GetMethods())
            {
                if(i.Name == "Invoke" && i.GetParameters().Length == 2)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.MethodInfoInvoke);
                }
            }
            mi = typeof(System.Type).GetMethod("GetTypeFromHandle");
            RegisterCLRMethodRedirection(mi, CLRRedirections.GetTypeFromHandle);
            mi = typeof(object).GetMethod("GetType");
            RegisterCLRMethodRedirection(mi, CLRRedirections.ObjectGetType);
            dMgr = new DelegateManager(this);
            dMgr.RegisterDelegateConvertor<Action>((dele) =>
            {
                return dele;
            });

            RegisterCrossBindingAdaptor(new Adaptors.AttributeAdaptor());

            debugService = new Debugger.DebugService(this);
        }

        public IType VoidType { get { return voidType; } }
        public IType IntType { get { return intType; } }
        public IType LongType { get { return longType; } }
        public IType BoolType { get { return boolType; } }
        public IType FloatType { get { return floatType; } }
        public IType DoubleType { get { return doubleType; } }
        public IType ObjectType { get { return objectType; } }

        public Dictionary<string, IType> LoadedTypes { get { return mapType; } }
        internal Dictionary<System.Reflection.MethodInfo, Func<ILContext, object, object[], IType[], object>> RedirectMap { get { return redirectMap; } }
        internal Dictionary<Type, CrossBindingAdaptor> CrossBindingAdaptors { get { return crossAdaptors; } }
        public DebugService DebugService { get { return debugService; } }
        internal Dictionary<int, ILIntepreter> Intepreters { get { return intepreters; } }
        internal Queue<ILIntepreter> FreeIntepreters { get { return freeIntepreters; } }

        public DelegateManager DelegateManager { get { return dMgr; } }
        public void LoadAssembly(System.IO.Stream stream)
        {
            LoadAssembly(stream, null, null);
        }
        public void LoadAssembly(System.IO.Stream stream, System.IO.Stream symbol, ISymbolReaderProvider symbolReader)
        {
            var module = ModuleDefinition.ReadModule(stream);

            if (symbolReader != null && symbol != null)
            {
                module.ReadSymbols(symbolReader.GetSymbolReader(module, symbol));
            }
            if (module.HasAssemblyReferences)
            {
                foreach (var ar in module.AssemblyReferences)
                {
                    /*if (moduleref.Contains(ar.Name) == false)
                        moduleref.Add(ar.Name);
                    if (moduleref.Contains(ar.FullName) == false)
                        moduleref.Add(ar.FullName);*/
                }
            }
            if (module.HasTypes)
            {
                List<ILType> types = new List<ILType>();
                foreach (var t in module.GetTypes())
                {
                    ILType type = new ILType(t, this);
                    mapType[t.FullName] = type;
                    types.Add(type);
                }
            }

            if (voidType == null)
            {
                voidType = GetType("System.Void");
                intType = GetType("System.Int32");
                longType = GetType("System.Int64");
                boolType = GetType("System.Boolean");
                floatType = GetType("System.Single");
                doubleType = GetType("System.Double");
                objectType = GetType("System.Object");
            }
            module.AssemblyResolver.ResolveFailure += AssemblyResolver_ResolveFailure;
#if DEBUG
            debugService.NotifyModuleLoaded(module.Name);
#endif
        }

        /// <summary>
        /// External reference should be added to the AppDomain by the method
        /// </summary>
        /// <param name="name">Assembly name, without .dll</param>
        /// <param name="content">file content</param>
        public void AddReferenceBytes(string name, byte[] content)
        {
            references[name] = content;
        }

        private AssemblyDefinition AssemblyResolver_ResolveFailure(object sender, AssemblyNameReference reference)
        {
            byte[] content;
            if (references.TryGetValue(reference.Name, out content))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(content))
                {
                    return AssemblyDefinition.ReadAssembly(ms);
                }
            }
            else
                return null;
        }

        public void RegisterCLRMethodRedirection(System.Reflection.MethodInfo mi, Func<ILContext, object, object[], IType[], object> func)
        {
            redirectMap[mi] = func;
        }
        
        public IType GetType(string fullname)
        {
            IType res;
            if (fullname == null)
            {
                return null;
            }
            if (mapType.TryGetValue(fullname, out res))
                return res;
            string baseType;
            List<string> genericParams;
            bool isArray;
            ParseGenericType(fullname, out baseType, out genericParams, out isArray);
            bool isByRef = baseType.EndsWith("&");
            if (isByRef)
                baseType = baseType.Substring(0, baseType.Length - 1);
            if (genericParams != null || isArray || isByRef)
            {
                IType bt = GetType(baseType);
                if (bt == null)
                {
                    bt = GetType(baseType.Replace("/", "+"));
                }

                if (bt == null)
                    return null;
                if (genericParams != null)
                {
                    KeyValuePair<string, IType>[] genericArguments = new KeyValuePair<string, IType>[genericParams.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        string key = "!" + i;
                        IType val = GetType(genericParams[i]);
                        if (val == null)
                            return null;
                        genericArguments[i] = new KeyValuePair<string, IType>(key, val);
                    }
                    bt = bt.MakeGenericInstance(genericArguments);
                    mapType[bt.FullName] = bt;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(baseType);
                    sb.Append('<');
                    for (int i = 0; i < genericParams.Count; i++)
                    {
                        if (i > 0)
                            sb.Append(",");
                        if (genericParams[i].Contains(","))
                            sb.Append(genericParams[i].Substring(0, genericParams[i].IndexOf(',')));
                        else
                            sb.Append(genericParams[i]);
                    }
                    sb.Append('>');
                    var asmName = sb.ToString();
                    if (bt.FullName != asmName)
                        mapType[asmName] = bt;
                }

                if (isByRef)
                {
                    bt = bt.MakeByRefType();
                    mapType[bt.FullName] = bt;
                    if (!isArray)
                    {
                        mapType[fullname] = bt;
                        return bt;
                    }
                }

                if (isArray)
                {
                    res = bt.MakeArrayType();
                    mapType[fullname] = res;
                    mapType[res.FullName] = res;
                    return res;
                }
                else
                    return bt;
            }
            else
            {
                Type t = Type.GetType(fullname);
                if (t != null)
                {
                    res = new CLRType(t, this);
                    mapType[fullname] = res;
                    mapType[res.FullName] = res;
                    mapType[t.AssemblyQualifiedName] = res;
                    clrTypeMapping[t] = res;
                    return res;
                }
            }
            return null;
        }

        static void ParseGenericType(string fullname, out string baseType, out List<string> genericParams, out bool isArray)
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;
            baseType = "";
            genericParams = null;
            if (fullname.Length >2 && fullname.Substring(fullname.Length - 2) == "[]")
            {
                fullname = fullname.Substring(0, fullname.Length - 2);
                isArray = true;
            }
            else
                isArray = false;
            if (fullname.Contains('<') || fullname.Contains('['))
            {
                foreach (var i in fullname)
                {
                    if (i == '<' || i == '[')
                    {
                        depth++;
                        if (depth == 1)
                        {
                            baseType = sb.ToString();
                            sb.Length = 0;
                            genericParams = new List<string>();
                            continue;
                        }
                    }
                    if (i == ',' && depth == 1)
                    {
                        string name = sb.ToString();
                        if (name.StartsWith("["))
                            name = name.Substring(1, name.Length - 2);
                        if (!string.IsNullOrEmpty(name))
                            genericParams.Add(name);
                        sb.Length = 0;
                        continue;
                    }
                    if (i == '>' || i == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string name = sb.ToString();
                            if (name.StartsWith("["))
                                name = name.Substring(1, name.Length - 2);
                            if (!string.IsNullOrEmpty(name))
                                genericParams.Add(name);
                            else
                            {
                                if (!isArray)
                                {
                                    isArray = true;
                                }
                                else
                                {
                                    baseType += "[]";
                                }
                            }
                            sb.Length = 0;
                            continue;
                        }
                    }
                    sb.Append(i);
                }
                if (sb.Length > 0)
                {
                    baseType += sb.ToString();
                }
                if (genericParams != null && genericParams.Count == 0)
                    genericParams = null;
            }
            else
                baseType = fullname;
        }

        string GetAssemblyName(IMetadataScope scope)
        {
            return scope is AssemblyNameReference ? ((AssemblyNameReference)scope).FullName : null;
        }

        internal IType GetType(object token, IType contextType)
        {
            int hash = token.GetHashCode();
            IType res;
            if (mapTypeToken.TryGetValue(hash, out res))
                return res;
            Mono.Cecil.ModuleDefinition module = null;
            KeyValuePair<string, IType>[] genericArguments = null;
            string typename = null;
            string scope = null;
            bool dummyGenericInstance = false;
            if (token is Mono.Cecil.TypeDefinition)
            {
                Mono.Cecil.TypeDefinition _def = (token as Mono.Cecil.TypeDefinition);
                module = _def.Module;
                typename = _def.FullName;
                scope = GetAssemblyName(_def.Scope);
            }
            else if (token is Mono.Cecil.TypeReference)
            {
                Mono.Cecil.TypeReference _ref = (token as Mono.Cecil.TypeReference);
                if (_ref.IsGenericParameter)
                {
                    var t = contextType.FindGenericArgument(_ref.Name);
                    return t;
                }
                if (_ref.IsByReference)
                {
                    var t = GetType(_ref.GetElementType(), contextType);
                    if (t != null)
                    {
                        res = t.MakeByRefType();
                        if (res is ILType)
                        {
                            ///Unify the TypeReference
                            ((ILType)res).TypeReference = _ref;
                        }
                        mapTypeToken[hash] = res;
                        mapType[res.FullName] = res;
                        return res;
                    }
                    return null;
                }
                if (_ref.IsArray)
                {
                    var t = GetType(_ref.GetElementType(), contextType);
                    if (t != null)
                    {
                        res = t.MakeArrayType();
                        if (res is ILType)
                        {
                            ///Unify the TypeReference
                            ((ILType)res).TypeReference = _ref;
                        }
                        mapTypeToken[hash] = res;
                        mapType[res.FullName] = res;
                        return res;
                    }
                    return t;
                }
                module = _ref.Module;
                if (_ref.IsGenericInstance)
                {
                    GenericInstanceType gType = (GenericInstanceType)_ref;
                    typename = gType.ElementType.FullName;
                    scope = GetAssemblyName(gType.ElementType.Scope);
                    TypeReference tr = gType.ElementType;
                    genericArguments = new KeyValuePair<string, IType>[gType.GenericArguments.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        string key = tr.GenericParameters[i].Name;
                        IType val;
                        if (gType.GenericArguments[i].IsGenericParameter)
                        {
                            val = contextType.FindGenericArgument(gType.GenericArguments[i].Name);
                            dummyGenericInstance = true;
                            if(val ==null)
                            {
                                return null;
                            }
                        }
                        else
                            val = GetType(gType.GenericArguments[i], contextType);
                        genericArguments[i] = new KeyValuePair<string, IType>(key, val);
                    }
                }
                else
                {
                    typename = _ref.FullName;
                    scope = GetAssemblyName(_ref.Scope);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            res = GetType(typename);
            if (res == null)
            {
                typename = typename.Replace("/", "+");
                res = GetType(typename);
            }
            if (res == null && scope != null)
                res = GetType(typename + ", " + scope);
            if (res == null)
            {
                if (scope != null)
                {
                    string aname = scope.Split(',')[0];
                    foreach (var i in loadedAssemblies)
                    {
                        if (aname == i.GetName().Name)
                        {
                            res = GetType(typename + ", " + i.FullName);
                            if (res != null)
                                break;
                        }
                    }
                }
                if (res == null)
                {
                    foreach (var j in loadedAssemblies)
                    {
                        res = GetType(typename + ", " + j.FullName);
                        if (res != null)
                            break;
                    }
                }
                if (res != null && scope != null)
                {
                    mapType[typename + ", " + scope] = res;
                }
            }
            if (res == null)
                throw new KeyNotFoundException("Cannot find Type:" + typename);
            if (genericArguments != null)
            {
                res = res.MakeGenericInstance(genericArguments);
                if (!dummyGenericInstance && res is ILType)
                {
                    ((ILType)res).TypeReference = (TypeReference)token;
                }
                mapType[res.FullName] = res;
            }
            if (!dummyGenericInstance)
                mapTypeToken[hash] = res;
            return res;
        }

        internal IType GetType(int hash)
        {
            IType res;
            if (mapTypeToken.TryGetValue(hash, out res))
                return res;
            else
                return null;
        }

        internal IType GetType(Type t)
        {
            IType res;
            if (clrTypeMapping.TryGetValue(t, out res))
                return res;
            else
                return null;
        }

        /// <summary>
        /// Create a instance of the specified type, which is inherited from a CLR Type
        /// </summary>
        /// <typeparam name="T">CLR Type</typeparam>
        /// <param name="type">Full Name of the type</param>
        /// <param name="args">Arguments for the constructor</param>
        /// <returns></returns>
        public T Instantiate<T>(string type, object[] args = null)
        {
            ILTypeInstance ins = Instantiate(type, args);
            return (T)ins.CLRInstance;
        }

        /// <summary>
        /// Create a instance of the specified type
        /// </summary>
        /// <param name="type">Full Name of the type</param>
        /// <param name="args">Arguments for the constructor</param>
        /// <returns></returns>
        public ILTypeInstance Instantiate(string type, object[] args = null)
        {
            IType t;
            if (mapType.TryGetValue(type, out t))
            {
                ILType ilType = t as ILType;
                if(ilType != null)
                {
                    bool hasConstructor = args != null && args.Length != 0;
                    var res = ilType.Instantiate(!hasConstructor);
                    if (hasConstructor)
                    {
                        var ilm = ilType.GetConstructor(args.Length);
                        Invoke(ilm, res, args);
                    }
                    return res;
                }
            }

            return null;
        }

        /// <summary>
        /// Invoke a method
        /// </summary>
        /// <param name="type">Type's fullname</param>
        /// <param name="method">Method name</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object Invoke(string type, string method, object instance, params object[] p)
        {
            IType t = GetType(type);
            if (t == null)
                return null;
            var m = t.GetMethod(method, p != null ? p.Length : 0);

            if (m != null)
            {
                return Invoke(m, instance, p);
            }
            return null;
        }

        /// <summary>
        /// Invoke a generic method
        /// </summary>
        /// <param name="type">Type's fullname</param>
        /// <param name="method">Method name</param>
        /// <param name="genericArguments">Generic Arguments</param>
        /// <param name="instance">Object Instance of the method</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object InvokeGenericMethod(string type, string method, IType[] genericArguments, object instance, params object[] p)
        {
            IType t = GetType(type);
            if (t == null)
                return null;
            var m = t.GetMethod(method, p.Length);

            if (m != null)
            {
                m = m.MakeGenericMethod(genericArguments);
                return Invoke(m, instance, p);
            }
            return null;
        }

        /// <summary>
        /// Invokes a specific method
        /// </summary>
        /// <param name="m">Method</param>
        /// <param name="instance">object instance</param>
        /// <param name="p">Parameters</param>
        /// <returns></returns>
        public object Invoke(IMethod m, object instance, params object[] p)
        {
            object res = null;
            if (m is ILMethod)
            {
                ILIntepreter inteptreter = null;
                lock (freeIntepreters)
                {
                    if (freeIntepreters.Count > 0)
                    {
                        inteptreter = freeIntepreters.Dequeue();
                        //Clear debug state, because it may be in ShouldBreak State
                        inteptreter.ClearDebugState();
                    }
                    else
                    {
                        inteptreter = new ILIntepreter(this);
#if DEBUG
                        intepreters[inteptreter.GetHashCode()] = inteptreter;
                        debugService.ThreadStarted(inteptreter);
#endif
                    }
                }
                try
                {
                    res = inteptreter.Run((ILMethod)m, instance, p);
                }
                finally
                {
                    lock (freeIntepreters)
                    {
#if DEBUG
                        if(inteptreter.CurrentStepType!= StepTypes.None)
                        {
                            //We should resume all other threads if we are currently doing stepping operation
                            foreach(var i in intepreters)
                            {
                                if(i.Value != inteptreter)
                                {
                                    i.Value.ClearDebugState();
                                    i.Value.Resume();
                                }
                            }
                            inteptreter.ClearDebugState();
                        }
#endif
                        inteptreter.Stack.ManagedStack.Clear();
                        inteptreter.Stack.Frames.Clear();
                        freeIntepreters.Enqueue(inteptreter);
#if DEBUG
                        //debugService.ThreadEnded(inteptreter);
#endif

                    }
                }
            }

            return res;
        }

        internal IMethod GetMethod(object token, ILType contextType,ILMethod contextMethod, out bool invalidToken)
        {
            string methodname = null;
            string typename = null;
            TypeReference typeDef = null;
            List<IType> paramList = null;
            int hashCode = token.GetHashCode();
            IMethod method;
            IType[] genericArguments = null;
            IType returnType;
            invalidToken = false;
            bool isConstructor = false;
            if (mapMethod.TryGetValue(hashCode, out method))
                return method;
            if (token is Mono.Cecil.MethodReference)
            {
                Mono.Cecil.MethodReference _ref = (token as Mono.Cecil.MethodReference);
                if (_ref.FullName == "System.Void System.Object::.ctor()")
                {
                    mapMethod[hashCode] = null;
                    return null;
                }
                if (_ref.FullName == "System.Void System.Attribute::.ctor()")
                {
                    mapMethod[hashCode] = null;
                    return null;
                }
                methodname = _ref.Name;
                typeDef = _ref.DeclaringType;
                if (token is Mono.Cecil.MethodDefinition)
                {
                    var def = _ref as MethodDefinition;
                    isConstructor = def.IsConstructor;
                }
                else
                    isConstructor = methodname == ".ctor";

                paramList = _ref.GetParamList(this, contextType);
                returnType = GetType(_ref.ReturnType, contextType);
                if (_ref.IsGenericInstance)
                {
                    GenericInstanceMethod gim = (GenericInstanceMethod)_ref;
                    genericArguments = new IType[gim.GenericArguments.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (gim.GenericArguments[i].IsGenericParameter)
                            invalidToken = true;
                        var gt = GetType(gim.GenericArguments[i], contextType);
                        if (gt == null)
                        {
                            gt = contextMethod.FindGenericArgument(gim.GenericArguments[i].Name);
                            if (gt == null)//This means it contains unresolved generic arguments, which means it's not searching the generic instance
                            {
                                genericArguments = null;
                                break;
                            }
                            else
                                genericArguments[i] = gt;
                        }
                        else
                            genericArguments[i] = gt;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
                //Mono.Cecil.GenericInstanceMethod gmethod = _def as Mono.Cecil.GenericInstanceMethod;
                //genlist = new MethodParamList(environment, gmethod);
            }

            var type = GetType(typeDef, contextType);
            if (type == null)
                throw new KeyNotFoundException("Cannot find type:" + typename);

            if (isConstructor)
                method = type.GetConstructor(paramList);
            else
            {
                method = type.GetMethod(methodname, paramList, genericArguments, returnType);
                if (method != null && method.IsGenericInstance)
                    mapMethod[method.GetHashCode()] = method;
            }

            if (method == null)
            {
                if (isConstructor && contextType.BaseType != null && contextType.BaseType is CrossBindingAdaptor && type.TypeForCLR == ((CrossBindingAdaptor)contextType.BaseType).BaseCLRType)
                {
                    method = contextType.BaseType.GetConstructor(paramList);
                    if(method == null)
                        throw new KeyNotFoundException("Cannot find method:" + methodname);
                    invalidToken = true;
                    mapMethod[method.GetHashCode()] = method;
                }
                else
                    throw new KeyNotFoundException("Cannot find method:" + methodname);
            }
            if (!invalidToken)
                mapMethod[hashCode] = method;
            return method;
        }

        internal IMethod GetMethod(int tokenHash)
        {
            IMethod res;
            if (mapMethod.TryGetValue(tokenHash, out res))
                return res;

            return null;
        }

        internal int GetFieldIndex(object token, IType contextType)
        {
            FieldReference f = token as FieldReference;
            var type = GetType(f.DeclaringType, contextType);
            if(type != null)
            {
                return type.GetFieldIndex(token);
            }
            throw new KeyNotFoundException();
        }

        internal long GetStaticFieldIndex(object token, IType contextType)
        {
            FieldReference f = token as FieldReference;
            var type = GetType(f.DeclaringType, contextType);

            if (type is ILType)
            {
                var it = type as ILType;
                int idx = it.GetFieldIndex(token);
                long res = ((long)it.TypeReference.GetHashCode() << 32) | (uint)idx;

                return res;
            }
            else
            {
                int idx = type.GetFieldIndex(token);
                long res = ((long)f.DeclaringType.GetHashCode() << 32) | (uint)idx;

                return res;
            }
        }

        internal void CacheString(object token)
        {
            int hashCode = token.GetHashCode();
            mapString[hashCode] = (string)token;
        }

        internal string GetString(int hashCode)
        {
            string res = null;
            if (mapString.TryGetValue(hashCode, out res))
                return res;
            return res;
        }

        public void RegisterCrossBindingAdaptor(CrossBindingAdaptor adaptor)
        {
            if (!crossAdaptors.ContainsKey(adaptor.BaseCLRType))
            {
                var t = adaptor.AdaptorType;
                var res = GetType(t);
                if (res == null)
                {
                    res = new CLRType(t, this);
                    mapType[res.FullName] = res;
                    mapType[t.AssemblyQualifiedName] = res;
                    clrTypeMapping[t] = res;
                }
                adaptor.RuntimeType = res;
                crossAdaptors[adaptor.BaseCLRType] = adaptor;
            }
        }
    }
}
