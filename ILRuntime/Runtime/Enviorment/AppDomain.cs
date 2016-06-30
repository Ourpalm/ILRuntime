using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        IType voidType, intType, boolType;
        public AppDomain()
        {
        }

        public IType VoidType { get { return voidType; } }
        public IType IntType { get { return intType; } }
        public IType BoolType { get { return boolType; } }

        public Dictionary<string, IType> LoadedTypes { get { return mapType; } }
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
                    ILType type = new ILType(t);
                    mapType[t.FullName] = type;
                    types.Add(type);
                }

                foreach (var t in types)
                {
                    t.InitializeBaseType(this);
                }
            }

            voidType = GetType("System.Void");
            intType = GetType("System.Int32");
            boolType = GetType("System.Boolean");
        }
        
        public IType GetType(string fullname)
        {
            IType res;
            if (mapType.TryGetValue(fullname, out res))
                return res;
            Type t = Type.GetType(fullname);
            if(t != null)
            {
                res = new CLRType(t, this);
                mapType[fullname] = res;
                ((CLRType)res).Initialize();
                return res;               
            }
            return null;
        }

        public IType GetType(object token, IType contextType)
        {
            int hash = token.GetHashCode();
            IType res;
            if (mapTypeToken.TryGetValue(hash, out res))
                return res;
            Mono.Cecil.ModuleDefinition module = null;
            string typename = null;
            if (token is Mono.Cecil.TypeDefinition)
            {
                Mono.Cecil.TypeDefinition _def = (token as Mono.Cecil.TypeDefinition);
                module = _def.Module;
                typename = _def.FullName;
            }
            else if (token is Mono.Cecil.TypeReference)
            {
                Mono.Cecil.TypeReference _ref = (token as Mono.Cecil.TypeReference);
                if (_ref.IsGenericParameter)
                {
                    /*foreach (var i in contextType.SubTypes)
                    {
                        if (i.Key == _ref.Name)
                            return i.Value;
                    }*/
                }
                module = _ref.Module;
                typename = _ref.FullName;
            }
            else
            {
                throw new NotImplementedException();
            }
            res = GetType(typename);
            mapTypeToken[hash] = res;
            return res;
        }

        public IType GetType(int hash)
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
                return inteptreter.Run((ILMethod)m, p);
            }
            return null;
        }

        public IMethod GetMethod(object token, ILType contextType)
        {
            string methodname = null;
            string typename = null;
            List<IType> paramList = null;
            int hashCode = token.GetHashCode();
            IMethod method;
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
                typename = _ref.DeclaringType.FullName;
                if (token is Mono.Cecil.MethodDefinition)
                {
                    var def = _ref as MethodDefinition;
                    isConstructor = def.IsConstructor;
                }
                else
                    isConstructor = methodname == ".ctor";

                paramList = _ref.GetParamList(this);
                if (_ref.IsGenericInstance)
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
                //Mono.Cecil.GenericInstanceMethod gmethod = _def as Mono.Cecil.GenericInstanceMethod;
                //genlist = new MethodParamList(environment, gmethod);
            }

            var type = GetType(typename);
            if (type == null)
                throw new KeyNotFoundException("Cannot find type:" + typename);

            if (isConstructor)
                method = type.GetConstructor(paramList);
            else
                method = type.GetMethod(methodname, paramList);

            if (method == null)
                throw new KeyNotFoundException("Cannot find method:" + methodname);
            mapMethod[hashCode] = method;
            return method;
        }

        public IMethod GetMethod(int tokenHash)
        {
            IMethod res;
            if (mapMethod.TryGetValue(tokenHash, out res))
                return res;

            return null;
        }

        public int GetFieldIndex(object token)
        {
            FieldDefinition f = token as FieldDefinition;
            var type = GetType(f.DeclaringType.FullName);
            if(type is ILType)
            {
                return ((ILType)type).GetFieldIndex(token);
            }
            throw new KeyNotFoundException();
        }

        public void CacheString(object token)
        {
            int hashCode = token.GetHashCode();
            mapString[hashCode] = (string)token;
        }

        public string GetString(int hashCode)
        {
            string res = null;
            if (mapString.TryGetValue(hashCode, out res))
                return res;
            return res;
        }
    }
}
