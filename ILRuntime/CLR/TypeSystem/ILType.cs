using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;

namespace ILRuntime.CLR.TypeSystem
{
    class ILType : IType
    {
        Dictionary<string, List<ILMethod>> methods;
        TypeDefinition definition;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public TypeDefinition TypeDefinition { get { return definition; } }

        public IType BaseType { get; private set; }

        public ILType(TypeDefinition def)
        {
            this.definition = def;
        }

        public bool IsGenericInstance
        {
            get
            {
                return false;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return typeof(ILTypeInstance);
            }
        }

        public void InitializeBaseType(Runtime.Enviorment.AppDomain domain)
        {
            appdomain = domain;
            if (definition.BaseType != null)
            {
                BaseType = domain.GetType(definition.BaseType.FullName);
                if (BaseType is CLRType)
                {
                    if (BaseType.TypeForCLR == typeof(Enum) || BaseType.TypeForCLR == typeof(object) || BaseType.TypeForCLR == typeof(ValueType) || BaseType.TypeForCLR == typeof(System.Enum))
                    {//都是这样，无所谓
                        BaseType = null;
                    }
                    else
                    {
                        //继承了其他系统类型
                        //env.logger.Log_Error("ScriptType:" + Name + " Based On a SystemType:" + BaseType.Name);
                        //HasSysBase = true;
                        //throw new Exception("不得继承系统类型，脚本类型系统和脚本类型系统是隔离的");
                    }
                }
                if (definition.HasInterfaces)
                {
                    /*_Interfaces = new List<ICLRType>();
                    bool bWarning = true;
                    foreach (var i in type_CLRSharp.Interfaces)
                    {
                        var itype = env.GetType(i.FullName);
                        if (itype is ICLRType_System)
                        {
                            //继承了其他系统类型
                            Type ts = (itype as ICLRType_System).TypeForSystem;

                            if (bWarning & env.GetCrossBind(ts) == null)
                            {

                                if (ts.IsInterface)
                                {
                                    foreach (var t in ts.GetInterfaces())
                                    {
                                        if (env.GetCrossBind(t) != null)
                                        {
                                            bWarning = false;
                                            break;
                                        }
                                    }
                                }
                                if (bWarning)
                                {
                                    env.logger.Log_Warning("警告:没有CrossBind的情况下直接继承\nScriptType:" + Name + " Based On a SystemInterface:" + itype.Name);
                                }
                            }
                            HasSysBase = true;
                        }
                        _Interfaces.Add(itype);
                    }*/
                }
            }
        }

        public IMethod GetMethod(string name, int paramCount)
        {
            if (methods == null)
                InitializeMethods();
            List<ILMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                foreach(var i in lst)
                {
                    if (i.ParameterCount == paramCount)
                        return i;
                }
            }
            return null;
        }

        void InitializeMethods()
        {
            methods = new Dictionary<string, List<ILMethod>>();
            foreach(var i in definition.Methods)
            {
                List<ILMethod> lst;
                if(!methods.TryGetValue(i.Name, out lst))
                {
                    lst = new List<ILMethod>();
                    methods[i.Name] = lst;
                }
                lst.Add(new ILMethod(i, appdomain));
            }
        }
    }
}
