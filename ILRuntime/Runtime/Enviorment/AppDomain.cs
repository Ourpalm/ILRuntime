using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Enviorment
{
    public class AppDomain
    {
        HashSet<string> loadedAssembly;
        Queue<ILIntepreter> freeIntepreters = new Queue<ILIntepreter>();
        Dictionary<string, IType> mapType = new Dictionary<string, IType>();

        public AppDomain()
        {
        }

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
                foreach (var t in module.Types)
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
        }
        
        public IType GetType(string fullname)
        {
            IType res;
            if (mapType.TryGetValue(fullname, out res))
                return res;
            Type t = Type.GetType(fullname);
            if(t != null)
            {
                res = new CLRType(t);
                mapType[fullname] = res;
                return res;               
            }
            return null;
        }

        public object Invoke(string type, string method, params object[] p)
        {
            IType t = GetType(type);
            var m = t.GetMethod(method, p.Length);

            if(m != null)
            {
                if(m is ILMethod)
                {
                    ILIntepreter inteptreter = null;
                    lock (freeIntepreters)
                    {
                        if (freeIntepreters.Count > 0)
                            inteptreter = freeIntepreters.Dequeue();
                        else
                            inteptreter = new ILIntepreter(this);
                    }
                    inteptreter.Run((ILMethod)m, p);
                }
            }
            return null;
        }
    }
}
