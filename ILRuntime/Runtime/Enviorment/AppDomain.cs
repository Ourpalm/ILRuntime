using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Enviorment
{
    public class AppDomain
    {
        HashSet<string> loadedAssembly;
        Queue<ILIntepreter> freeIntepreters = new Queue<ILIntepreter>();
        Dictionary<string, IType> mapType = new Dictionary<string, IType>();
        Dictionary<int, IType> mapTypeToken = new Dictionary<int, IType>();
        Dictionary<int, IMethod> mapMethod = new Dictionary<int, IMethod>();
        Dictionary<int, string> mapString = new Dictionary<int, string>();
        Dictionary<System.Reflection.MethodInfo, Func<object, object[], IType[], object>> redirectMap = new Dictionary<System.Reflection.MethodInfo, Func<object, object[], IType[], object>>();
        IType voidType, intType, longType, boolType, floatType, doubleType, objectType;
        public AppDomain()
        {
            var mi = typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray");
            RegisterCLRMethodRedirection(mi, CLRRedirections.InitializeArray);
            foreach (var i in typeof(System.Activator).GetMethods())
            {
                if (i.Name == "CreateInstance" && i.IsGenericMethodDefinition)
                {
                    RegisterCLRMethodRedirection(i, CLRRedirections.CreateInstance);
                }
            }
            
        }

        internal IType VoidType { get { return voidType; } }
        internal IType IntType { get { return intType; } }
        internal IType LongType { get { return longType; } }
        internal IType BoolType { get { return boolType; } }
        internal IType FloatType { get { return floatType; } }
        internal IType DoubleType { get { return doubleType; } }
        internal IType ObjectType { get { return objectType; } }

        public Dictionary<string, IType> LoadedTypes { get { return mapType; } }
        internal Dictionary<System.Reflection.MethodInfo, Func<object, object[], IType[], object>> RedirectMap { get { return redirectMap; } }
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

            voidType = GetType("System.Void");
            intType = GetType("System.Int32");
            longType = GetType("System.Int64");
            boolType = GetType("System.Boolean");
            floatType = GetType("System.Single");
            doubleType = GetType("System.Double");
            objectType = GetType("System.Object");
        }

        public void RegisterCLRMethodRedirection(System.Reflection.MethodInfo mi, Func<object, object[], IType[], object> func)
        {
            redirectMap[mi] = func;
        }
        
        public IType GetType(string fullname)
        {
            IType res;
            if (mapType.TryGetValue(fullname, out res))
                return res;
            string baseType;
            string[] genericParams;
            bool isArray;
            ParseGenericType(fullname, out baseType, out genericParams, out isArray);
            if (genericParams != null || isArray)
            {
                IType bt = GetType(baseType);

                if (genericParams != null)
                {
                    KeyValuePair<string, IType>[] genericArguments = new KeyValuePair<string, IType>[genericParams.Length];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        string key = "!" + i;
                        IType val = GetType(genericParams[i]);
                        genericArguments[i] = new KeyValuePair<string, IType>(key, val);
                    }
                    bt = bt.MakeGenericInstance(genericArguments);
                    mapType[bt.FullName] = bt;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(baseType);
                    sb.Append('<');
                    for (int i = 0; i < genericParams.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(", ");
                        else
                        {
                            sb.Append(genericParams[i]);
                        }
                    }
                    sb.Append('>');
                    var asmName = sb.ToString();
                    if (bt.FullName != asmName)
                        mapType[asmName] = bt;
                }

                if (isArray)
                {
                    Type t = bt.TypeForCLR.MakeArrayType();
                    res = new CLRType(t, this);
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
                    return res;
                }
            }
            return null;
        }

        static void ParseGenericType(string fullname, out string baseType, out string[] genericParams, out bool isArray)
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;
            int idx = 0;
            int cnt = 0;
            baseType = "";
            genericParams = null;
            if (fullname.Length >2 && fullname.Substring(fullname.Length - 2) == "[]")
            {
                fullname = fullname.Substring(0, fullname.Length - 2);
                isArray = true;
            }
            else
                isArray = false;
            if (fullname.Contains('<'))
            {
                foreach (var i in fullname)
                {
                    if (i == '<')
                    {
                        depth++;
                        if (depth == 1)
                        {
                            baseType = sb.ToString();
                            sb.Length = 0;
                            cnt = int.Parse(baseType.Substring(baseType.IndexOf('`') + 1));
                            genericParams = new string[cnt];
                            continue;
                        }
                    }
                    if (i == ',' && depth == 1)
                    {
                        genericParams[idx++] = sb.ToString();
                        sb.Length = 0;
                    }
                    if (i == '>')
                    {
                        depth--;
                        if (depth == 0)
                            continue;
                    }
                    sb.Append(i);
                }
                if (sb.Length > 0)
                    genericParams[idx] = sb.ToString();
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
                    var t = ((ILType)contextType).FindGenericArgument(_ref.Name);
                    if (t != null)
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
                            val = ((ILType)contextType).FindGenericArgument(gType.GenericArguments[i].Name);
                            dummyGenericInstance = true;
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
            if (res == null && scope != null)
                res = GetType(typename + ", " + scope);
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

        public object Invoke(string type, string method, params object[] p)
        {
            IType t = GetType(type);
            var m = t.GetMethod(method, p.Length);

            if(m != null)
            {
                return Invoke(m, p);
            }
            return null;
        }

        public object Invoke(IMethod m, params object[] p)
        {
            if (m is ILMethod)
            {
                ILIntepreter inteptreter = null;
                lock (freeIntepreters)
                {
                    if (freeIntepreters.Count > 0)
                        inteptreter = freeIntepreters.Dequeue();
                    else
                        inteptreter = new ILIntepreter(this);
                }
                var res = inteptreter.Run((ILMethod)m, p);
                lock (freeIntepreters)
                {
                    freeIntepreters.Enqueue(inteptreter);
                }
                return inteptreter.Run((ILMethod)m, p);
            }
            return null;
        }

        internal IMethod GetMethod(object token, ILType contextType)
        {
            string methodname = null;
            string typename = null;
            TypeReference typeDef = null;
            List<IType> paramList = null;
            int hashCode = token.GetHashCode();
            IMethod method;
            IType[] genericArguments = null;
            bool invalidToken = false;
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
                if (_ref.IsGenericInstance)
                {
                    GenericInstanceMethod gim = (GenericInstanceMethod)_ref;
                    genericArguments = new IType[gim.GenericArguments.Count];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (gim.GenericArguments[i].IsGenericParameter)
                            invalidToken = true;
                        genericArguments[i] = GetType(gim.GenericArguments[i], contextType);
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
                method = type.GetMethod(methodname, paramList, genericArguments);
                if (method.IsGenericInstance)
                    mapMethod[method.GetHashCode()] = method;
            }

            if (method == null)
                throw new KeyNotFoundException("Cannot find method:" + methodname);
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
            FieldDefinition f = token as FieldDefinition;
            var type = GetType(f.DeclaringType, contextType);
            if(type is ILType)
            {
                return ((ILType)type).GetFieldIndex(token);
            }
            throw new KeyNotFoundException();
        }

        internal long GetStaticFieldIndex(object token, IType contextType)
        {
            FieldReference f = token as FieldReference;
            var type = GetType(f.DeclaringType, contextType);
            
            if (type is ILType)
            {
                var it = (ILType)type;
                int idx = it.GetFieldIndex(token);
                long res = ((long)it.TypeReference.GetHashCode() << 32) | (uint)idx;

                return res;
            }
            throw new KeyNotFoundException();
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
    }
}
