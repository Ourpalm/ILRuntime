using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.CLR.TypeSystem
{
    class ILType : IType
    {
        Dictionary<string, List<ILMethod>> methods;
        TypeReference typeRef;
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
        KeyValuePair<string, IType>[] genericArguments;
        IType baseType;
        bool baseTypeInitialized = false;
        List<ILType> genericInstances;
        public TypeDefinition TypeDefinition { get { return definition; } }

        public TypeReference TypeReference
        {
            get { return typeRef; }
            set
            {
                typeRef = value;
                if (value is GenericInstanceType)
                    definition = (TypeDefinition)((GenericInstanceType)value).ElementType;
                else
                    definition = (TypeDefinition)value;
            }
        }

        public IType BaseType
        {
            get
            {
                if (!baseTypeInitialized)
                    InitializeBaseType();
                return baseType;
            }
        }

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

        public ILType(TypeReference def, Runtime.Enviorment.AppDomain domain)
        {
            this.typeRef = def;
            if (def is GenericInstanceType)
                definition = (TypeDefinition)((GenericInstanceType)def).ElementType;
            else
                definition = (TypeDefinition)def;
            appdomain = domain;            
        }

        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }
        public KeyValuePair<string, IType>[] GenericArguments
        {
            get
            {
                return genericArguments;
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

        string genericFullName;
        public string FullName
        {
            get
            {
                return typeRef.FullName;
                /*if (genericArguments == null)
                    return definition.FullName;
                else
                {
                    if (string.IsNullOrEmpty(genericFullName))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(definition.FullName);
                        sb.Append('<');
                        for (int i = 0; i < genericArguments.Length; i++)
                        {
                            if (i > 1)
                                sb.Append(", ");
                            sb.Append(genericArguments[i].Value.FullName);
                        }
                        sb.Append('>');
                        genericFullName = sb.ToString();
                    }
                    return genericFullName;
                }*/
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
        void InitializeBaseType()
        {
            if (definition.BaseType != null)
            {
                baseType = appdomain.GetType(definition.BaseType, this);
                if (baseType is CLRType)
                {
                    if (baseType.TypeForCLR == typeof(Enum) || baseType.TypeForCLR == typeof(object) || baseType.TypeForCLR == typeof(ValueType) || baseType.TypeForCLR == typeof(System.Enum))
                    {//都是这样，无所谓
                        baseType = null;
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
            var m = GetMethod(method.Name, method.Parameters, null);
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

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments)
        {
            if (methods == null)
                InitializeMethods();
            List<ILMethod> lst;
            IMethod genericMethod = null;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    int pCnt = param != null ? param.Count : 0;
                    if (i.ParameterCount == pCnt)
                    {
                        bool match = true;
                        if (genericArguments != null && i.GenericParameterCount == genericArguments.Length)
                        {
                            genericMethod = i;
                        }
                        else
                        {
                            for (int j = 0; j < pCnt; j++)
                            {
                                if (param[j] != i.Parameters[j])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                                return i;
                        }
                    }
                }
            }
            if (genericArguments != null && genericMethod != null)
            {
                var m = genericMethod.MakeGenericMethod(genericArguments);
                lst.Add((ILMethod)m);
                return m;
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
                {
                    bool match = true;
                    if (genericArguments != null && i.GenericParameterCount == genericArguments.Length)

                        for (int j = 0; j < param.Count; j++)
                        {
                            if (param[j] != i.Parameters[j])
                            {
                                match = false;
                                break;
                            }
                        }
                    if (match)
                        return i;
                }
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
            FieldReference f = token as FieldReference;
            if (staticFieldMapping != null && staticFieldMapping.TryGetValue(f.Name, out idx))
            {
                fieldTokenMapping[hashCode] = idx;
                return idx;
            }
            if (fieldMapping.TryGetValue(f.Name, out idx))
            {
                fieldTokenMapping[hashCode] = idx;
                return idx;
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
                    if (field.FieldType.IsGenericParameter)
                    {
                        staticFieldTypes[idxStatic] = FindGenericArgument(field.FieldType.Name);
                    }
                    else
                        staticFieldTypes[idxStatic] = appdomain.GetType(field.FieldType, this);
                    idxStatic++;
                }
                else
                {
                    fieldMapping[field.Name] = idx;
                    if (field.FieldType.IsGenericParameter)
                    {
                        fieldTypes[idx - FieldStartIndex] = FindGenericArgument(field.FieldType.Name);
                    }
                    else
                        fieldTypes[idx - FieldStartIndex] = appdomain.GetType(field.FieldType, this);
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

        public IType FindGenericArgument(string key)
        {
            foreach (var i in genericArguments)
            {
                if (i.Key == key)
                    return i.Value;
            }
            return null;
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
        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            if (genericInstances == null)
                genericInstances = new List<ILType>();
            foreach (var i in genericInstances)
            {
                bool match = true;
                for (int j = 0; j < genericArguments.Length; j++)
                {
                    if(i.genericArguments[j].Value != genericArguments[j].Value)
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            var res = new ILType(definition, appdomain);
            res.genericArguments = genericArguments;

            genericInstances.Add(res);
            return res;
        }

        public IType ResolveGenericType(IType contextType)
        {
            var ga = contextType.GenericArguments;
            IType[] kv = new IType[definition.GenericParameters.Count];
            for (int i = 0; i < kv.Length; i++)
            {
                var gp = definition.GenericParameters[i];
                string name = gp.Name;
                foreach (var j in ga)
                {
                    if (j.Key == name)
                    {
                        kv[i] = j.Value;
                        break;
                    }
                }
            }

            foreach (var i in genericInstances)
            {
                bool match=true;
                for (int j = 0; j < kv.Length; j++)
                {
                    if (i.genericArguments[j].Value != kv[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }

            return null;
        }
    }
}
