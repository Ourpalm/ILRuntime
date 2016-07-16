using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.CLR.TypeSystem
{
    class ILType : IType
    {
        Dictionary<string, List<ILMethod>> methods;
        TypeDefinition definition;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        ILMethod staticConstructor;
        List<ILMethod> constructors;
        IType[] fieldTypes;
        IType[] staticFieldTypes;
        Dictionary<string, int> fieldMapping;
        Dictionary<string, int> staticFieldMapping;
        ILTypeStaticInstance staticInstance;
        Dictionary<int, int> fieldTokenMapping = new Dictionary<int, int>();
        int fieldStartIdx = -1;
        int totalFieldCnt = -1;
        bool staticInitialized = false;
        public TypeDefinition TypeDefinition { get { return definition; } }

        public IType BaseType { get; private set; }

        public ILTypeStaticInstance StaticInstance { get { return staticInstance; } }

        public IType[] FieldTypes
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return fieldTypes;
            }
        }

        public IType[] StaticFieldTypes
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields(); 
                return staticFieldTypes;
            }
        }
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        int FieldStartIndex
        {
            get
            {
                if (fieldStartIdx < 0)
                {
                    if (BaseType != null)
                    {
                        if (BaseType is ILType)
                        {
                            fieldStartIdx = ((ILType)BaseType).TotalFieldCount;
                        }
                    }
                    else
                        fieldStartIdx = 0;
                }
                return fieldStartIdx;
            }
        }

        public int TotalFieldCount
        {
            get
            {
                if (totalFieldCnt < 0)
                {
                    if (fieldMapping == null)
                        InitializeFields();
                    if (BaseType != null)
                    {
                        if (BaseType is ILType)
                        {
                            totalFieldCnt = ((ILType)BaseType).TotalFieldCount + fieldTypes.Length;
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                        totalFieldCnt = fieldTypes.Length;
                }
                return totalFieldCnt;
            }
        }

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

        public bool IsValueType
        {
            get
            {
                return definition.IsValueType;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return typeof(ILTypeInstance);
            }
        }
        public string FullName
        {
            get
            {
                return definition.FullName;
            }
        }
        public List<IMethod> GetMethods()
        {
            List<IMethod> res = new List<IMethod>();
            foreach (var i in methods)
            {
                foreach (var j in i.Value)
                    res.Add(j);
            }

            return res;
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
                        throw new NotImplementedException();
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
            constructors = new List<ILMethod>();
            foreach(var i in definition.Methods)
            {
                if (i.IsConstructor)
                {
                    if (i.IsStatic)
                        staticConstructor = new ILMethod(i, this, appdomain);
                    else
                        constructors.Add(new ILMethod(i, this, appdomain));
                }
                else
                {
                    List<ILMethod> lst;
                    if (!methods.TryGetValue(i.Name, out lst))
                    {
                        lst = new List<ILMethod>();
                        methods[i.Name] = lst;
                    }
                    var m = new ILMethod(i, this, appdomain);
                    lst.Add(new ILMethod(i, this, appdomain));
                }
            }

            if (staticConstructor != null)
            {
                appdomain.Invoke(staticConstructor);
            }
        }

        public IMethod GetVirtualMethod(IMethod method)
        {
            var m = GetMethod(method.Name, method.Parameters);
            if (m == null)
            {
                if (BaseType != null)
                {
                    if (BaseType is ILType)
                    {
                        return ((ILType)BaseType).GetVirtualMethod(method);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                    return null;
            }
            else
                return m;
        }

        public IMethod GetMethod(string name, List<IType> param)
        {
            if (methods == null)
                InitializeMethods();
            List<ILMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    int pCnt = param != null ? param.Count : 0;
                    if (i.ParameterCount == pCnt)
                    {
                        bool match = true;
                        for (int j = 0; j < pCnt; j++)
                        {
                            if (param[j] != i.Parameters[j])
                                match = false;
                        }
                        if (match)
                            return i;
                    }
                }
            }
            return null;
        }

        public IMethod GetConstructor(List<IType> param)
        {
            if (constructors == null)
                InitializeMethods();
            foreach(var i in constructors)
            {
                if (i.ParameterCount == param.Count)
                    return i;
            }
            return null;
        }

        public int GetFieldIndex(object token)
        {
            if (fieldMapping == null)
                InitializeFields();
            int idx;
            int hashCode = token.GetHashCode();
            if (fieldTokenMapping.TryGetValue(hashCode, out idx))
                return idx;
            FieldDefinition f = token as FieldDefinition;
            if (f.IsStatic)
            {
                if (staticFieldMapping.TryGetValue(f.Name, out idx))
                {
                    fieldTokenMapping[hashCode] = idx;
                    return idx;
                }
            }
            else
            {
                if (fieldMapping.TryGetValue(f.Name, out idx))
                {
                    fieldTokenMapping[hashCode] = idx;
                    return idx;
                }
            }
            return -1;
        }

        void InitializeFields()
        {
            fieldMapping = new Dictionary<string, int>();
            fieldTypes = new IType[definition.Fields.Count];
            var fields = definition.Fields;
            int idx = FieldStartIndex;
            int idxStatic = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.IsStatic)
                {
                    if (staticFieldTypes == null)
                    {
                        staticFieldTypes = new IType[definition.Fields.Count];
                        staticFieldMapping = new Dictionary<string, int>();
                    }
                    staticFieldMapping[field.Name] = idxStatic;
                    staticFieldTypes[idxStatic] = appdomain.GetType(field.FieldType.FullName);
                    idxStatic++;
                }
                else
                {
                    fieldMapping[field.Name] = idx;
                    fieldTypes[idx - FieldStartIndex] = appdomain.GetType(field.FieldType.FullName);
                    idx++;
                }
            }
            Array.Resize(ref fieldTypes, idx - FieldStartIndex);

            if (staticFieldTypes != null)
            {
                Array.Resize(ref staticFieldTypes, idxStatic);
                staticInstance = new ILTypeStaticInstance(this);
            }
        }

        public bool CanAssignTo(IType type)
        {
            if (this == type)
            {
                return true;
            }
            else if (BaseType != null)
                return BaseType.CanAssignTo(type);
            else
                return false;
        }

        internal ILTypeInstance Instantiate()
        {
            return new ILTypeInstance(this);
        }
    }
}
