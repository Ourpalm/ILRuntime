using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Reflection;

namespace ILRuntime.CLR.TypeSystem
{
    public class ILType : IType
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
        IType baseType, byRefType, arrayType, enumType;
        IType[] interfaces;
        bool baseTypeInitialized = false;
        bool interfaceInitialized = false;
        List<ILType> genericInstances;
        bool isDelegate;
        ILRuntimeType reflectionType;
        public TypeDefinition TypeDefinition { get { return definition; } }

        public TypeReference TypeReference
        {
            get { return typeRef; }
            set
            {
                typeRef = value;
                RetriveDefinitino(value);
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

        public IType[] Implements
        {
            get
            {
                if (!interfaceInitialized)
                    InitializeInterfaces();
                return interfaces;
            }
        }

        public ILTypeStaticInstance StaticInstance
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                if (methods == null)
                    InitializeMethods();
                return staticInstance;
            }
        }

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

        public Dictionary<string, int> FieldMapping { get { return fieldMapping; } }

        public Dictionary<string,int> StaticFieldMapping { get { return staticFieldMapping; } }
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        internal int FieldStartIndex
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
                        else
                            fieldStartIdx = 0;
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
                            totalFieldCnt = fieldTypes.Length;
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
            RetriveDefinitino(def);
            appdomain = domain;
        }

        void RetriveDefinitino(TypeReference def)
        {
            TypeReference res;
            if (def is GenericInstanceType)
                res = ((GenericInstanceType)def).ElementType;
            else if (def is ByReferenceType)
            {
                res = ((ByReferenceType)def).ElementType;
            }
            else if (def.IsArray)
            {
                res = (TypeDefinition)def.GetElementType();
            }
            else
                res = (TypeDefinition)def;
            if (!res.IsGenericParameter)
            {
                definition = res as TypeDefinition;
            }
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

        public bool IsDelegate
        {
            get
            {
                if (!baseTypeInitialized)
                    InitializeBaseType();
                return isDelegate;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                if (!baseTypeInitialized)
                    InitializeBaseType();
                if (definition.IsEnum)
                {
                    if (enumType == null)
                        InitializeFields();
                    if (enumType == null)
                    {

                    }
                    return enumType.TypeForCLR;
                }
                else if(baseType != null && baseType is CrossBindingAdaptor)
                {
                    return ((CrossBindingAdaptor)baseType).RuntimeType.TypeForCLR;
                }
                else
                    return typeof(ILTypeInstance);
            }
        }

        public Type ReflectionType
        {
            get
            {
                if (reflectionType == null)
                    reflectionType = new ILRuntimeType(this);
                return reflectionType;
            }
        }

        public IType ByRefType
        {
            get
            {
                return byRefType;
            }
        }
        public IType ArrayType
        {
            get
            {
                return arrayType;
            }
        }

        public bool IsEnum
        {
            get
            {
                return definition.IsEnum;
            }
        }
        public string FullName
        {
            get
            {
                return typeRef.FullName;
            }
        }
        public string Name
        {
            get
            {
                return typeRef.Name;
            }
        }
        public List<IMethod> GetMethods()
        {
            if (methods == null)
                InitializeMethods();
            List<IMethod> res = new List<IMethod>();
            foreach (var i in methods)
            {
                foreach (var j in i.Value)
                    res.Add(j);
            }

            return res;
        }
        void InitializeInterfaces()
        {
            interfaceInitialized = true;
            if (definition.HasInterfaces)
            {
                interfaces = new IType[definition.Interfaces.Count];
                for (int i = 0; i < interfaces.Length; i++)
                {
                    interfaces[i] = appdomain.GetType(definition.Interfaces[i], this);
                    if(interfaces[i] is CLRType)
                    {
                        CrossBindingAdaptor adaptor;
                        if (appdomain.CrossBindingAdaptors.TryGetValue(interfaces[i].TypeForCLR, out adaptor))
                        {
                            interfaces[i] = adaptor;
                        }
                        else
                            throw new TypeLoadException("Cannot find Adaptor for:" + interfaces[i].TypeForCLR.ToString());
                    }
                }
            }
        }
        void InitializeBaseType()
        {
            baseTypeInitialized = true;
            if (definition.BaseType != null)
            {
                baseType = appdomain.GetType(definition.BaseType, this);
                if (baseType is CLRType)
                {
                    if (baseType.TypeForCLR == typeof(Enum) || baseType.TypeForCLR == typeof(object) || baseType.TypeForCLR == typeof(ValueType) || baseType.TypeForCLR == typeof(System.Enum))
                    {//都是这样，无所谓
                        baseType = null;
                    }
                    else if(baseType.TypeForCLR == typeof(MulticastDelegate))
                    {
                        baseType = null;
                        isDelegate = true;
                    }
                    else
                    {
                        CrossBindingAdaptor adaptor;
                        if (appdomain.CrossBindingAdaptors.TryGetValue(baseType.TypeForCLR, out adaptor))
                        {
                            baseType = adaptor;
                        }
                        else
                            throw new TypeLoadException("Cannot find Adaptor for:" + baseType.TypeForCLR.ToString());
                        //继承了其他系统类型
                        //env.logger.Log_Error("ScriptType:" + Name + " Based On a SystemType:" + BaseType.Name);
                        //HasSysBase = true;
                        //throw new Exception("不得继承系统类型，脚本类型系统和脚本类型系统是隔离的");
                    }
                }
            }
        }

        public IMethod GetMethod(string name)
        {
            if (methods == null)
                InitializeMethods();
            List<ILMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                return lst[0];
            }
            return null;
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
                appdomain.Invoke(staticConstructor, null, null);
            }
        }

        public IMethod GetVirtualMethod(IMethod method)
        {
            var m = GetMethod(method.Name, method.Parameters, null, method.ReturnType);
            if (m == null)
            {
                if (BaseType != null)
                {
                    return BaseType.GetVirtualMethod(method);
                }
                else
                    return null;//BaseType == null means base type is Object or Enum
            }
            else
                return m;
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null)
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
                            genericMethod = CheckGenericParams(i, param, ref match);
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
                            {
                                match = returnType == null || i.ReturnType == returnType;
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

        ILMethod CheckGenericParams(ILMethod i, List<IType> param, ref bool match)
        {
            ILMethod genericMethod = null;
            for (int j = 0; j < param.Count; j++)
            {
                var p = i.Parameters[j];
                if (p is CLR.TypeSystem.ILGenericParameterType)
                {
                    //TODO should match the generic parameters;
                    continue;
                }
                if (param[j] != p)
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                genericMethod = i;
            }
            return genericMethod;
        }

        public IMethod GetConstructor(int paramCnt)
        {
            if (constructors == null)
                InitializeMethods();
            foreach (var i in constructors)
            {
                if (i.ParameterCount == paramCnt)
                {
                    return i;
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
                {
                    bool match = true;

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
                    if (IsEnum)
                    {
                        enumType = fieldTypes[idx - FieldStartIndex];
                    }
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
            if (genericArguments != null)
            {
                foreach (var i in genericArguments)
                {
                    if (i.Key == key)
                        return i.Value;
                }
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
            else if (Implements != null)
            {
                for (int i = 0; i < interfaces.Length; i++)
                {
                    var im = interfaces[i];
                    bool res = im.CanAssignTo(type);
                    if (res)
                        return true;
                }
            }
            return false;
        }

        public ILTypeInstance Instantiate(bool callDefaultConstructor = true)
        {
            var res = new ILTypeInstance(this);
            if (callDefaultConstructor)
            {
                var m = GetConstructor(CLR.Utils.Extensions.EmptyParamList);
                if (m != null)
                {
                    appdomain.Invoke(m, res, null);
                }
            }
            return res;
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

        public IType MakeByRefType()
        {
            if (byRefType == null)
            {
                var def = new ByReferenceType(typeRef);
                byRefType = new ILType(def, appdomain);
            }
            return byRefType;
        }

        public IType MakeArrayType()
        {
            if (arrayType == null)
            {
                var def = new ArrayType(typeRef);
                arrayType = new ILType(def, appdomain);
            }
            return arrayType;
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
