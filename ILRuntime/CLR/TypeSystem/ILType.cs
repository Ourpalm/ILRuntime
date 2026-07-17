using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Stack;
using System.Runtime.CompilerServices;

namespace ILRuntime.CLR.TypeSystem
{
#if ENABLE_NEO_MODE
    internal struct ILTypeFieldOffset
    {
        public int PrimitiveOffset;
        public int ReferenceOffset;
    }
#endif
    public sealed class ILType : IType
    {
        Dictionary<string, List<ILMethod>> methods;
        TypeReference typeRef;
        TypeDefinition definition;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        bool staticConstructorCalled;
        ILMethod staticConstructor;
        List<ILMethod> constructors;
        IType [] fieldTypes;
#if ENABLE_NEO_MODE
        ILTypeFieldOffset[] fieldOffsets;
        ILTypeFieldOffset[] staticFieldOffsets;
        IMethod[] neoVTable;
        Dictionary<IMethod, int> neoVTableSlots;
        Dictionary<IType, int> neoInterfaceOffsets;
        bool neoVTableBuilding;
#endif
        FieldReference[] fieldReferences;
        FieldDefinition[] fieldDefinitions;
        IType[] staticFieldTypes;
        FieldReference[] staticFieldReferences;
        FieldDefinition[] staticFieldDefinitions;
        Dictionary<string, int> fieldMapping;
        Dictionary<string, int> staticFieldMapping;
        ILTypeStaticInstance staticInstance;
        Dictionary<int, int> fieldTokenMapping = new Dictionary<int, int> ();
        int fieldStartIdx = -1;
        int totalFieldCnt = -1;
        bool hasGenericArguments;
        KeyValuePair<string, IType> [] genericArguments;
        IType baseType, byRefType, enumType, elementType;
        Dictionary<int, IType> arrayTypes;
        Type arrayCLRType, byRefCLRType;
        IType [] interfaces;
        bool baseTypeInitialized = false;
        bool interfaceInitialized = false;
        List<ILType> genericInstances;
        bool isDelegate;
        ILRuntimeType reflectionType;
        ILType genericDefinition;
        IType firstCLRBaseType, firstCLRInterface;
        int hashCode = -1;
        int tIdx = -1;
        static int instance_id = 0x10000000;
        int jitFlags;
        public TypeDefinition TypeDefinition { get { return definition; } }
        bool mToStringGot, mEqualsGot, mGetHashCodeGot;
        IMethod mToString, mEquals, mGetHashCode;
        int valuetypeFieldCount, valuetypeManagedCount;
        bool valuetypeSizeCalculated;
        ValueTypeInitInfo vtInitInfo;
#if ENABLE_NEO_MODE
        int totalPrimitiveSize = -1;
        int totalReferenceCnt = -1;
        int totalStaticPrimitiveSize = -1;
        int totalStaticReferenceCnt = -1;
        internal int[] refFieldPrimitiveOffsets;
#endif

        public IMethod ToStringMethod
        {
            get
            {
                if ( !mToStringGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "ToString", 0, true );
                    mToString = GetVirtualMethod ( m );
                    mToStringGot = true;
                }
                return mToString;
            }
        }

        public IMethod EqualsMethod
        {
            get
            {
                if ( !mEqualsGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "Equals", 1, true );
                    mEquals = GetVirtualMethod ( m );
                    mEqualsGot = true;
                }
                return mEquals;
            }
        }

        public IMethod GetHashCodeMethod
        {
            get
            {
                if ( !mGetHashCodeGot )
                {
                    IMethod m = appdomain.ObjectType.GetMethod ( "GetHashCode", 0, true );
                    mGetHashCode = GetVirtualMethod ( m );
                    mGetHashCodeGot = true;
                }
                return mGetHashCode;
            }
        }

        public TypeReference TypeReference
        {
            get { return typeRef; }
            set
            {
                typeRef = value;
                RetriveDefinitino ( value );
            }
        }

        public IType BaseType
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return baseType;
            }
        }

        public IType [] Implements
        {
            get
            {
                if ( !interfaceInitialized )
                    InitializeInterfaces ();
                return interfaces;
            }
        }

        public ILTypeStaticInstance StaticInstance
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                if ( methods == null )
                    InitializeMethods ();
                if ( staticInstance == null && staticFieldTypes != null )
                {
                    staticInstance = new ILTypeStaticInstance ( this );
                }
                if ( staticInstance != null && !staticConstructorCalled )
                {
                    staticConstructorCalled = true;
                    if ( staticConstructor != null && ( !TypeReference.HasGenericParameters || IsGenericInstance ) )
                    {
                        appdomain.Invoke ( staticConstructor, null, null );
                    }
                }
                return staticInstance;
            }
        }

        public IType [] FieldTypes
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return fieldTypes;
            }
        }

        public IType [] StaticFieldTypes
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return staticFieldTypes;
            }
        }

        public FieldDefinition [] StaticFieldDefinitions
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields ();
                return staticFieldDefinitions;
            }
        }

        public FieldReference[] StaticFieldReferences
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return staticFieldReferences;
            }
        }

        public Dictionary<string, int> FieldMapping
        {
            get
            {
                if ( fieldMapping == null )
                    InitializeFields (); return fieldMapping;
            }
        }

        public IType FirstCLRBaseType
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return firstCLRBaseType;
            }
        }

        public IType FirstCLRInterface
        {
            get
            {
                if ( !interfaceInitialized )
                    InitializeInterfaces ();
                return firstCLRInterface;
            }
        }
        public bool HasGenericParameter
        {
            get
            {
                if (genericArguments != null)
                    return hasGenericArguments;
                return typeRef.HasGenericParameters && genericArguments == null;
            }
        }

        public bool IsGenericParameter
        {
            get
            {
                return typeRef.IsGenericParameter && genericArguments == null;
            }
        }

        public Dictionary<string, int> StaticFieldMapping { get { return staticFieldMapping; } }
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
                if ( fieldStartIdx < 0 )
                {
                    if ( BaseType != null )
                    {
                        if ( BaseType is ILType )
                        {
                            fieldStartIdx = ( ( ILType ) BaseType ).TotalFieldCount;
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
                if ( totalFieldCnt < 0 )
                {
                    if ( fieldMapping == null )
                        InitializeFields ();
                    if ( BaseType != null )
                    {
                        if ( BaseType is ILType )
                        {
                            totalFieldCnt = ( ( ILType ) BaseType ).TotalFieldCount + fieldTypes.Length;
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

#if ENABLE_NEO_MODE
        public IMethod[] NeoVTable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNeoVTable();
                return neoVTable;
            }
        }

        internal bool TryGetNeoVTableSlot(IMethod method, out int slot)
        {
            EnsureNeoVTable();
            if (method != null && neoVTableSlots != null)
                return neoVTableSlots.TryGetValue(method, out slot);
            slot = -1;
            return false;
        }

        internal bool TryGetNeoInterfaceOffset(IType interfaceType, out int baseSlot)
        {
            EnsureNeoVTable();
            if (interfaceType != null && neoInterfaceOffsets != null)
                return neoInterfaceOffsets.TryGetValue(interfaceType, out baseSlot);
            baseSlot = -1;
            return false;
        }

        internal bool TryGetInterfaceMethodSlot(IMethod interfaceMethod, out int slot)
        {
            EnsureNeoVTable();
            if (interfaceMethod != null && neoVTableSlots != null)
                return neoVTableSlots.TryGetValue(interfaceMethod, out slot);
            slot = -1;
            return false;
        }

        public int TotalPrimitiveSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // fieldMapping is the first thing InitializeFields() assigns,
                // and it stays non-null afterwards. Use it as the
                // initialized-flag so cyclic field-type graphs (e.g. a struct
                // referenced indirectly by its own static field type) don't
                // re-enter InitializeFields and overflow the stack.
                if (fieldMapping == null)
                {
                    InitializeFields();
                }
                return totalPrimitiveSize;
            }
        }

        public int TotalReferenceCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                {
                    InitializeFields();
                }
                return totalReferenceCnt;
            }
        }

        public int StaticTotalPrimitiveSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                {
                    InitializeFields();
                }
                return totalStaticPrimitiveSize;
            }
        }

        public int StaticTotalReferenceCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                {
                    InitializeFields();
                }
                return totalStaticReferenceCnt;
            }
        }

        void EnsureNeoVTable()
        {
            if (neoVTable == null)
                BuildNeoVTable();
        }

        void BuildNeoVTable()
        {
            if (neoVTableBuilding)
                throw new InvalidOperationException(string.Format("Recursive Neo VTable build detected for type {0}", FullName));

            neoVTableBuilding = true;
            try
            {
                if (methods == null)
                    InitializeMethods();

                List<IMethod> slots = new List<IMethod>();
                Dictionary<IMethod, int> slotMap = new Dictionary<IMethod, int>();
                Dictionary<IType, int> interfaceOffsets = null;

                if (!IsValueType)
                {
                    if (IsInterface)
                    {
                        BuildNeoInterfaceOwnSlots(slots, slotMap);
                    }
                    else
                    {
                        // Base class slot 阶段
                        IType baseForVTable = BaseType;
                        if (baseForVTable is ILType baseILType)
                        {
                            var baseTable = baseILType.NeoVTable;
                            for (int i = 0; i < baseTable.Length; i++)
                            {
                                var baseMethod = baseTable[i];
                                slots.Add(baseMethod);
                                if (baseMethod != null && !slotMap.ContainsKey(baseMethod))
                                    slotMap.Add(baseMethod, i);
                            }

                            if (baseILType.neoInterfaceOffsets != null && baseILType.neoInterfaceOffsets.Count > 0)
                            {
                                interfaceOffsets = new Dictionary<IType, int>(baseILType.neoInterfaceOffsets);
                            }
                        }
                        else
                        {
                            if (baseForVTable == null)
                                baseForVTable = appdomain.ObjectType;
                            AddNeoBaseVirtualSlots(baseForVTable, slots, slotMap);
                        }

                        // 本类阶段：新增虚方法 + override 覆盖
                        HashSet<ILMethod> added = new HashSet<ILMethod>();
                        foreach (var pair in methods)
                        {
                            foreach (var method in pair.Value)
                            {
                                if (!added.Add(method) || !IsNeoVTableCandidate(method))
                                    continue;

                                int slot = FindNeoOverrideSlot(method, slots, slotMap);
                                if (slot >= 0)
                                {
                                    // 用本类方法覆盖 slot。**保留基类 IMethod → slot 映射**：
                                    // 运行时 callvirt 的 declaredMethod 可能是基类版本（如 CLR object.ToString），
                                    // 仍要能通过 slotMap 查到 slot。
                                    slots[slot] = method;
                                    slotMap[method] = slot;
                                }
                                else
                                {
                                    AddNeoVTableSlot(method, slots, slotMap);
                                }
                            }
                        }

                        // 接口阶段：
                        //  ① 从基类继承的 neoInterfaceOffsets 里，若本类 override 了对应实现方法，重新填充接口块 slot；
                        //  ② 为本类直接声明的接口分配新的 slot 块。
                        if (interfaceOffsets != null && interfaceOffsets.Count > 0)
                        {
                            // 复制一份 key，避免遍历时修改
                            var inheritedIfaces = new List<KeyValuePair<IType, int>>(interfaceOffsets);
                            foreach (var kv in inheritedIfaces)
                            {
                                if (!(kv.Key is ILType ilIface) || !ilIface.IsInterface)
                                    continue;
                                var ifaceVT = ilIface.NeoVTable;
                                int baseSlot = kv.Value;
                                for (int i = 0; i < ifaceVT.Length; i++)
                                {
                                    IMethod ifaceMethod = ifaceVT[i];
                                    IMethod impl = ResolveNeoInterfaceImplementation(ifaceMethod, ilIface);
                                    if (impl == null || ReferenceEquals(impl, ifaceMethod) ||
                                        (impl.DeclearingType != null && impl.DeclearingType.IsInterface))
                                        continue;  // 保持基类 slot 已有值
                                    int idx = baseSlot + i;
                                    if (idx >= 0 && idx < slots.Count)
                                        slots[idx] = impl;
                                }
                            }
                        }

                        var directInterfaces = Implements;
                        if (directInterfaces != null)
                        {
                            for (int idx = 0; idx < directInterfaces.Length; idx++)
                            {
                                IType iface = directInterfaces[idx];
                                if (!(iface is ILType ilIface) || !ilIface.IsInterface)
                                    continue;

                                if (interfaceOffsets == null)
                                    interfaceOffsets = new Dictionary<IType, int>();

                                // 若基类已声明同一个接口，此处仍然为本类重新分配 slot 块，
                                // 覆盖继承 offset，确保 slot 指向本类最新 impl（含 override）。
                                int baseSlot = slots.Count;
                                interfaceOffsets[ilIface] = baseSlot;

                                var ifaceVTable = ilIface.NeoVTable;
                                for (int i = 0; i < ifaceVTable.Length; i++)
                                {
                                    IMethod ifaceMethod = ifaceVTable[i];
                                    IMethod impl = ResolveNeoInterfaceImplementation(ifaceMethod, ilIface);
                                    if (impl == null || ReferenceEquals(impl, ifaceMethod) ||
                                        (impl.DeclearingType != null && impl.DeclearingType.IsInterface))
                                    {
                                        throw new TypeLoadException(string.Format(
                                            "Type {0} does not implement interface method {1}.{2}",
                                            FullName, ilIface.FullName, ifaceMethod != null ? ifaceMethod.Name : "<null>"));
                                    }
                                    slots.Add(impl);
                                }

                                // 父接口继承：接口 VTable 前段是父接口方法（BuildNeoInterfaceOwnSlots 保证顺序）。
                                // 为父接口注册同一个 baseSlot（父方法在 slot [baseSlot, baseSlot+parentSize) 内），
                                // 使运行时通过父接口 IType 也能命中同一份实现。
                                RegisterParentInterfaceOffsets(ilIface, baseSlot, interfaceOffsets);
                            }
                        }
                    }
                }

                neoVTable = slots.ToArray();
                neoVTableSlots = slotMap;
                neoInterfaceOffsets = interfaceOffsets;
            }
            finally
            {
                neoVTableBuilding = false;
            }
        }

        void BuildNeoInterfaceOwnSlots(List<IMethod> slots, Dictionary<IMethod, int> slotMap)
        {
            // 递归展开父接口，父接口 slot 前置
            var parents = Implements;
            if (parents != null)
            {
                for (int idx = 0; idx < parents.Length; idx++)
                {
                    IType parent = parents[idx];
                    if (!(parent is ILType parentIface) || !parentIface.IsInterface)
                        continue;
                    var parentVT = parentIface.NeoVTable;
                    for (int i = 0; i < parentVT.Length; i++)
                    {
                        IMethod pm = parentVT[i];
                        if (pm == null || slotMap.ContainsKey(pm))
                            continue;
                        slotMap.Add(pm, slots.Count);
                        slots.Add(pm);
                    }
                }
            }

            // 本接口自身方法
            HashSet<ILMethod> added = new HashSet<ILMethod>();
            foreach (var pair in methods)
            {
                foreach (var method in pair.Value)
                {
                    if (!added.Add(method) || !IsNeoVTableCandidate(method))
                        continue;
                    if (slotMap.ContainsKey(method))
                        continue;
                    slotMap.Add(method, slots.Count);
                    slots.Add(method);
                }
            }
        }

        void AddNeoBaseVirtualSlots(IType baseType, List<IMethod> slots, Dictionary<IMethod, int> slotMap)
        {
            if (baseType == null)
                return;

            foreach (var method in baseType.GetMethods())
            {
                if (!IsNeoVTableCandidate(method))
                    continue;
                AddNeoVTableSlot(method, slots, slotMap);
            }
        }

        static void AddNeoVTableSlot(IMethod method, List<IMethod> slots, Dictionary<IMethod, int> slotMap)
        {
            if (method == null || slotMap.ContainsKey(method))
                return;

            slotMap.Add(method, slots.Count);
            slots.Add(method);
        }

        void RegisterParentInterfaceOffsets(ILType ilIface, int childBaseSlot, Dictionary<IType, int> interfaceOffsets)
        {
            // BuildNeoInterfaceOwnSlots 保证：接口 VTable 前段是父接口方法（按 Implements 顺序展开）。
            // 但父方法在 child slot 内的起始位置 = 该父接口在 child 里排在前面所有父接口的方法数总和。
            // 简化：递归展开，每个父接口的 slot 起点 = childBaseSlot + 父接口方法在 child VTable 里的实际起始位置。
            var parents = ilIface.Implements;
            if (parents == null) return;
            var childVT = ilIface.NeoVTable;
            for (int p = 0; p < parents.Length; p++)
            {
                if (!(parents[p] is ILType parentIface) || !parentIface.IsInterface)
                    continue;
                var parentVT = parentIface.NeoVTable;
                if (parentVT.Length == 0)
                    continue;
                // 在 child VTable 中找父接口第一个方法的位置（IMethod 引用相等）。
                int startInChild = -1;
                for (int i = 0; i < childVT.Length; i++)
                {
                    if (ReferenceEquals(childVT[i], parentVT[0]))
                    {
                        startInChild = i;
                        break;
                    }
                }
                if (startInChild < 0)
                    continue;
                int parentBaseSlot = childBaseSlot + startInChild;
                // 覆盖式写入：本类多个直接接口都涉及同一父接口时，取最后一个（任意一个即可，都指向本类实现）。
                interfaceOffsets[parentIface] = parentBaseSlot;
                RegisterParentInterfaceOffsets(parentIface, parentBaseSlot, interfaceOffsets);
            }
        }

        IMethod ResolveNeoInterfaceImplementation(IMethod ifaceMethod, ILType ilIface)
        {
            // 显式接口实现优先（C# 规范：接口调用应走显式实现，即使存在同名隐式实现）。
            // Legacy GetVirtualMethod 在 GetMethod(name,...) 命中隐式实现后不再查显式命名 fallback，
            // 这里显式重试 "{Iface.FullNameForNested}.{Name}" 命名 lookup。
            if (ifaceMethod != null)
            {
                string explicitName = string.Format("{0}.{1}", ilIface.FullNameForNested, ifaceMethod.Name);
                IType[] genericArgs = null;
                if (ifaceMethod.IsGenericInstance)
                {
                    if (ifaceMethod is ILMethod ilm)
                        genericArgs = ilm.GenericArugmentsArray;
                    else if (ifaceMethod is CLRMethod clrm)
                        genericArgs = clrm.GenericArguments;
                }
                var explicitImpl = GetMethod(explicitName, ifaceMethod.Parameters, genericArgs, ifaceMethod.ReturnType, true);
                if (explicitImpl != null)
                    return explicitImpl;
            }

            return this.GetVirtualMethod(ifaceMethod);
        }

        int FindNeoOverrideSlot(ILMethod method, List<IMethod> slots, Dictionary<IMethod, int> slotMap)
        {
            if (method.Definition.HasOverrides)
            {
                foreach (var overrideRef in method.Definition.Overrides)
                {
                    IMethod overrideMethod = null;
                    try
                    {
                        bool invalidToken;
                        overrideMethod = appdomain.GetMethod(overrideRef, this, method, out invalidToken);
                    }
                    catch
                    {
                    }

                    if (overrideMethod != null && slotMap.TryGetValue(overrideMethod, out int slot))
                        return slot;
                }
            }

            // ② 隐式 override（!IsNewSlot）：借用 Legacy BaseType.GetVirtualMethod 拿基类方法
            if (!method.Definition.IsNewSlot && BaseType != null)
            {
                IMethod baseMethod = BaseType.GetVirtualMethod(method);
                if (baseMethod != null && !ReferenceEquals(baseMethod, method) &&
                    slotMap.TryGetValue(baseMethod, out int slot))
                    return slot;
            }

            return -1;
        }

        static bool IsNeoVTableCandidate(IMethod method)
        {
            if (method == null || method.IsStatic || method.IsConstructor)
                return false;

            if (method is ILMethod ilMethod)
                return ilMethod.Definition.IsVirtual;

            if (method is CLRMethod clrMethod)
            {
                var info = clrMethod.MethodInfo;
                return info != null && info.IsVirtual && !info.IsStatic;
            }

            return false;
        }

#endif

        internal List<ILType> GenericInstances
        {
            get
            {
                return genericInstances;
            }
        }

        /// <summary>
        /// 初始化IL类型
        /// </summary>
        /// <param name="def">MONO返回的类型定义</param>
        /// <param name="domain">ILdomain</param>
        public ILType ( TypeReference def, Runtime.Enviorment.AppDomain domain )
        {
            this.typeRef = def;
            RetriveDefinitino ( def );
            appdomain = domain;
            jitFlags = domain.DefaultJITFlags;
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="def"></param>
        void RetriveDefinitino ( TypeReference def )
        {
            if ( !def.IsGenericParameter && definition == null )
            {
                if ( def is TypeSpecification )
                {
                    if ( def.IsByReference || def is ArrayType )
                    {
                        definition = null;
                    }
                    else
                        RetriveDefinitino ( ( ( TypeSpecification ) def ).ElementType );
                }
                else
                    definition = def as TypeDefinition;
            }
        }

        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public ILType GetGenericDefinition ()
        {
            return genericDefinition;
        }
        public KeyValuePair<string, IType> [] GenericArguments
        {
            get
            {
                return genericArguments;
            }
        }

        public IType ElementType { get { return elementType; } }

        public bool IsArray
        {
            get; private set;
        }

        public int ArrayRank
        {
            get; private set;
        }

        public bool IsByRef
        {
            get
            {
                return typeRef.IsByReference;
            }
        }

        private bool? isValueType;

        public bool IsValueType
        {
            get
            {
                if ( IsArray )
                    return false;
                if ( isValueType == null )
                    isValueType = definition.IsValueType;

                return isValueType.Value;
            }
        }

        public bool IsDelegate
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                return isDelegate;
            }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsInterface
        {
            get
            {
                return TypeDefinition.IsInterface;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                if ( !baseTypeInitialized )
                    InitializeBaseType ();
                if ( typeRef is ArrayType )
                {
                    return arrayCLRType;
                }
                else if ( typeRef is ByReferenceType )
                {
                    return byRefCLRType;
                }
                else if ( this.IsEnum )
                {
                    if ( enumType == null )
                        InitializeFields ();
                    return enumType.TypeForCLR;
                }
                else if ( FirstCLRBaseType != null && FirstCLRBaseType is CrossBindingAdaptor )
                {
                    return ( ( CrossBindingAdaptor ) FirstCLRBaseType ).RuntimeType.TypeForCLR;
                }
                else if ( FirstCLRInterface != null && FirstCLRInterface is CrossBindingAdaptor )
                {
                    return ( ( CrossBindingAdaptor ) FirstCLRInterface ).RuntimeType.TypeForCLR;
                }
                else
                    return typeof ( ILTypeInstance );
            }
        }

        public Type ReflectionType
        {
            get
            {
                if ( reflectionType == null )
                    reflectionType = new ILRuntimeType ( this );
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
                return arrayTypes != null ? arrayTypes [ 1 ] : null;
            }
        }

        public bool IsEnum
        {
            get
            {
                return definition != null ? definition.IsEnum : false;
            }
        }

        string fullName, fullNameForNested;

        public string FullNameForNested
        {
            get
            {
                if ( string.IsNullOrEmpty ( fullNameForNested ) )
                {
                    if ( typeRef.IsNested )
                    {
                        fullNameForNested = FullName.Replace ( "/", "." );
                    }
                    else
                        fullNameForNested = FullName;
                }
                return fullNameForNested;
            }
        }

        public string FullName
        {
            get
            {
                if ( string.IsNullOrEmpty ( fullName ) )
                {
                    if ( typeRef.HasGenericParameters && genericArguments != null )
                    {
                        StringBuilder sb = new StringBuilder ();
                        sb.Append ( typeRef.FullName );
                        sb.Append ( '<' );
                        bool first = true;
                        foreach ( var i in genericArguments )
                        {
                            if ( first )
                                first = false;
                            else
                                sb.Append ( ", " );
                            sb.Append ( i.Value.FullName );
                        }
                        sb.Append ( '>' );
                        fullName = sb.ToString ();
                    }
                    else
                        fullName = typeRef.FullName;
                    /* 
                    if (typeRef.IsNested)
                    {
                        fullNameForNested = fullName.Replace("/", ".");
                    }
                    else
                        fullNameForNested = fullName;
                    */
                }
                return fullName;
            }
        }
        public string Name
        {
            get
            {
                return typeRef.Name;
            }
        }

        public StackObject DefaultObject { get { return default(StackObject); } }
        public int TypeIndex
        {
            get
            {
                if (tIdx < 0)
                    tIdx = appdomain.AllocTypeIndex(this);
                return tIdx;
            }
        }

        public List<IMethod> GetMethods ()
        {
            if ( methods == null )
                InitializeMethods ();
            List<IMethod> res = new List<IMethod> ();
            foreach ( var i in methods )
            {
                foreach ( var j in i.Value )
                    res.Add ( j );
            }

            return res;
        }
        void InitializeInterfaces ()
        {
            interfaceInitialized = true;
            if ( definition != null && definition.HasInterfaces )
            {
                interfaces = new IType [ definition.Interfaces.Count ];
                for ( int i = 0; i < interfaces.Length; i++ )
                {
                    interfaces [ i ] = appdomain.GetType ( definition.Interfaces [ i ].InterfaceType, this, null );
                    //only one clrInterface is valid
                    if ( interfaces [ i ] is CLRType && firstCLRInterface == null )
                    {
                        CrossBindingAdaptor adaptor;
                        if ( appdomain.CrossBindingAdaptors.TryGetValue ( interfaces [ i ].TypeForCLR, out adaptor ) )
                        {
                            interfaces [ i ] = adaptor;
                            firstCLRInterface = adaptor;
                        }
                        else
                            throw new TypeLoadException ( "Cannot find Adaptor for:" + interfaces [ i ].TypeForCLR.ToString () );
                    }
                }
            }
            if ( firstCLRInterface == null && BaseType != null && BaseType is ILType )
                firstCLRInterface = ( ( ILType ) BaseType ).FirstCLRInterface;
        }
        void InitializeBaseType ()
        {
            if ( definition != null && definition.BaseType != null )
            {
                bool specialProcess = false;
                List<int> spIdx = null;
                if ( definition.BaseType.IsGenericInstance )
                {
                    GenericInstanceType git = definition.BaseType as GenericInstanceType;
                    var elementType = appdomain.GetType ( git.ElementType, this, null );
                    if ( elementType is CLRType )
                    {
                        for ( int i = 0; i < git.GenericArguments.Count; i++ )
                        {
                            var ga = git.GenericArguments [ i ];
                            if ( ga == typeRef )
                            {
                                specialProcess = true;
                                if ( spIdx == null )
                                    spIdx = new List<int> ();
                                spIdx.Add ( i );
                            }
                        }
                    }
                }
                if ( specialProcess )
                {
                    //如果泛型参数是自身，则必须要特殊处理，否则会StackOverflow
                    var elementType = appdomain.GetType ( ( ( GenericInstanceType ) definition.BaseType ).ElementType, this, null );
                    foreach ( var i in appdomain.CrossBindingAdaptors )
                    {
                        if ( i.Key.IsGenericType && !i.Key.IsGenericTypeDefinition )
                        {
                            var gd = i.Key.GetGenericTypeDefinition ();
                            if ( gd == elementType.TypeForCLR )
                            {
                                var ga = i.Key.GetGenericArguments ();
                                bool match = true;
                                foreach ( var j in spIdx )
                                {
                                    if ( ga [ j ] != i.Value.AdaptorType )
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if ( match )
                                {
                                    baseType = i.Value;
                                    break;
                                }
                            }
                        }
                    }
                    if ( baseType == null )
                    {
                        throw new TypeLoadException ( "Cannot find Adaptor for:" + definition.BaseType.FullName );
                    }
                }
                else
                {
                    baseType = appdomain.GetType ( definition.BaseType, this, null );
                    if ( baseType is CLRType )
                    {
                        if ( baseType.TypeForCLR == typeof ( Enum ) || baseType.TypeForCLR == typeof ( object ) || baseType.TypeForCLR == typeof ( ValueType ) || baseType.TypeForCLR == typeof ( System.Enum ) )
                        {//都是这样，无所谓
                            baseType = null;
                        }
                        else if ( baseType.TypeForCLR == typeof ( MulticastDelegate ) )
                        {
                            baseType = null;
                            isDelegate = true;
                        }
                        else
                        {
                            CrossBindingAdaptor adaptor;
                            if ( appdomain.CrossBindingAdaptors.TryGetValue ( baseType.TypeForCLR, out adaptor ) )
                            {
                                baseType = adaptor;
                            }
                            else
                                throw new TypeLoadException ( "Cannot find Adaptor for:" + baseType.TypeForCLR.ToString () );
                            //继承了其他系统类型
                            //env.logger.Log_Error("ScriptType:" + Name + " Based On a SystemType:" + BaseType.Name);
                            //HasSysBase = true;
                            //throw new Exception("不得继承系统类型，脚本类型系统和脚本类型系统是隔离的");
                        }
                    }
                }
            }
            var curBase = baseType;
            while ( curBase is ILType )
            {
                curBase = curBase.BaseType;
            }
            firstCLRBaseType = curBase;
            baseTypeInitialized = true;
        }

        internal IMethod GetMethod(MethodDefinition def)
        {
            if (methods == null)
                InitializeMethods();
            if (def.IsConstructor)
            {
                foreach(var i in constructors)
                {
                    if (i.Definition == def)
                        return i;
                }
            }
            else
            {
                foreach(var i in  methods)
                {
                    foreach(var j in i.Value)
                    {
                        if(j.Definition == def)
                        {
                            return j;
                        }
                    }
                }
            }
            return null;
        }

        public ILMethod GetMethodByGenericDefinition(ILMethod definitionMethod)
        {
            if (definitionMethod == null)
                return null;
            if (methods == null)
                InitializeMethods();
            foreach(var i in methods)
            {
                foreach(var j in i.Value)
                {
                    if (j.Definition == definitionMethod.Definition)
                        return j;
                }
            }
            return null;
        }

        public IMethod GetMethod ( string name )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                return lst [ 0 ];
            }
            return null;
        }

        public IMethod GetMethod ( string name, int paramCount, bool declaredOnly = false )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                foreach ( var i in lst )
                {
                    if ( i.ParameterCount == paramCount )
                        return i;
                }
            }
            if ( declaredOnly )
                return null;
            else
            {
                //skip clr base type, this doesn't make any sense
                if ( BaseType != null && !( BaseType is CrossBindingAdaptor ) )
                    return BaseType.GetMethod ( name, paramCount, false );
                else
                    return null;
            }
        }

        void InitializeMethods()
        {
            methods = new Dictionary<string, List<ILMethod>>();
            constructors = new List<ILMethod>();
            if (definition == null)
                return;
            if (definition.HasCustomAttributes)
            {
                for (int i = 0; i < definition.CustomAttributes.Count; i++)
                {
                    int f;
                    if (definition.CustomAttributes[i].GetJITFlags(AppDomain, out f))
                    {
                        this.jitFlags = f;
                        break;
                    }
                }
            }
            foreach (var i in definition.Methods)
            {
                if (i.IsConstructor)
                {
                    if (i.IsStatic)
                        staticConstructor = new ILMethod(i, i, this, appdomain, jitFlags);
                    else
                        constructors.Add(new ILMethod(i, i, this, appdomain, jitFlags));
                }
                else
                {
                    List<ILMethod> lst;
                    var m = new ILMethod(i, i, this, appdomain, jitFlags);
                    if (!methods.TryGetValue(i.Name, out lst))
                    {
                        lst = new List<ILMethod>();
                        methods[i.Name] = lst;
                    }
                    lst.Add(m);
                    if (i.HasOverrides)
                    {
                        //Deal with interface implementation with explicit naming and implicit naming
                        foreach(var o in i.Overrides)
                        {
                            if (o.Name != i.Name)
                            {
                                if (!methods.TryGetValue(o.Name, out lst))
                                {
                                    lst = new List<ILMethod>();
                                    methods[o.Name] = lst;
                                }
                                lst.Add(m);
                            }
                            else
                            {
                                string cn = $"{o.DeclaringType.FullName}.{o.Name}";
                                if (cn != i.Name)
                                {
                                    if (!methods.TryGetValue(cn, out lst))
                                    {
                                        lst = new List<ILMethod>();
                                        methods[cn] = lst;
                                    }
                                    lst.Add(m);
                                }
                            }
                        }
                    }
                    
                }
            }

            foreach (var i in definition.Events)
            {
                int fieldIdx = -1;
                InitializeFields();
                if(i.AddMethod.IsStatic)
                    staticFieldMapping.TryGetValue(i.Name,out fieldIdx);
                else
                    fieldMapping.TryGetValue(i.Name, out fieldIdx);
                if (methods.TryGetValue(i.AddMethod.Name, out var lst))
                {
                    lst[0].SetEventAddOrRemove(true, false, fieldIdx);
                }
                if (methods.TryGetValue(i.RemoveMethod.Name, out lst))
                {
                    lst[0].SetEventAddOrRemove(false, true, fieldIdx);
                }
            }

            if (!appdomain.SuppressStaticConstructor && !staticConstructorCalled)
            {
                staticConstructorCalled = true;
                if (staticConstructor != null && (!TypeReference.HasGenericParameters || IsGenericInstance))
                {
#if ENABLE_NEO_MODE
                    // TODO Step 7: see InitializeMethods entry above. Re-enable once
                    // Neo Stfld_*/Ldfld_* handlers are wired up.
#else
                    appdomain.Invoke(staticConstructor, null, null);
#endif
                }
            }
        }

        public IMethod GetVirtualMethod ( IMethod method )
        {
            IType [] genericArguments = null;
            if ( method.IsGenericInstance )
            {
                if ( method is ILMethod )
                {
                    genericArguments = ( ( ILMethod ) method ).GenericArugmentsArray;
                }
                else
                {
                    genericArguments = ( ( CLRMethod ) method ).GenericArguments;
                }
            }

            var m = GetMethod ( method.Name, method.Parameters, genericArguments, method.ReturnType, true );
            if ( m == null && BaseType != null )
            {
                m = BaseType.GetVirtualMethod ( method );
                if ( m != null )
                    return m;
            }
            if ( m == null && method.DeclearingType.IsInterface )
            {
                if (method.DeclearingType is ILType)
                {
                    ILType iltype = (ILType)method.DeclearingType;
                    m = GetMethod(string.Format("{0}.{1}", iltype.FullNameForNested, method.Name), method.Parameters, genericArguments, method.ReturnType, true);
                }
                else
                {
                    ((CLRType)method.DeclearingType).TypeForCLR.GetClassName(out var clsName, out var realName, out var isByRef);
                    m = GetMethod(string.Format("{0}.{1}", realName, method.Name), method.Parameters, genericArguments, method.ReturnType, true);
                }
            }

            if ( m == null || m.IsGenericInstance == method.IsGenericInstance )
                return m;
            else
                return method;

        }

        bool CheckTypeEqual(IType typeA, IType typeB)
        {
            if (typeA is ILGenericParameterType pt1 && typeB is ILGenericParameterType pt2)
                return pt1 == pt2 || pt1.TypeReference == pt2.TypeReference || pt1.Name == pt2.Name;
            else
                return typeA == typeB;
        }

        public IMethod GetMethod ( string name, List<IType> param, IType [] genericArguments, IType returnType = null, bool declaredOnly = false )
        {
            if ( methods == null )
                InitializeMethods ();
            List<ILMethod> lst;
            IMethod genericMethod = null;
            if ( methods.TryGetValue ( name, out lst ) )
            {
                for ( var idx = 0; idx < lst.Count; idx++ )
                {
                    var i = lst [ idx ];
                    int pCnt = param != null ? param.Count : 0;
                    if ( i.ParameterCount == pCnt )
                    {
                        bool match = true;
                        if ( genericArguments != null && i.GenericParameterCount == genericArguments.Length && genericMethod == null )
                        {
                            genericMethod = CheckGenericParams ( i, param, genericArguments, ref match );
                        }
                        else
                        {
                            match = CheckGenericArguments ( i, genericArguments );
                            if ( !match )
                                continue;
                            for ( int j = 0; j < pCnt; j++ )
                            {
                                if (!CheckTypeEqual(param[j], i.Parameters[j]))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if ( match )
                            {
                                match = returnType == null || CheckTypeEqual(i.ReturnType, returnType);
                            }
                            if ( match )
                                return i;
                        }
                    }
                }
            }
            if ( genericArguments != null && genericMethod != null )
            {
                var m = genericMethod.MakeGenericMethod ( genericArguments );
                lst.Add ( ( ILMethod ) m );
                return m;
            }
            if ( declaredOnly )
                return null;
            else
            {
                if ( BaseType != null )
                    return BaseType.GetMethod ( name, param, genericArguments, returnType, false );
                else
                    return null;
            }
        }

        bool CheckGenericArguments ( ILMethod i, IType [] genericArguments )
        {
            if ( genericArguments == null )
            {
                return i.GenericArguments == null;
            }
            else
            {
                if ( i.GenericArguments == null )
                    return false;
                else if ( i.GenericArguments.Length != genericArguments.Length )
                    return false;
                if ( i.GenericArguments.Length == genericArguments.Length )
                {
                    for ( int j = 0; j < genericArguments.Length; j++ )
                    {
                        if ( i.GenericArguments [ j ].Value != genericArguments [ j ] )
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        bool IsGenericArgumentMatch ( IType p, IType p2, IType [] genericArguments )
        {
            bool found = false;
            foreach ( var a in genericArguments )
            {
                if ( a == p2 )
                {
                    found = true;
                    break;
                }
            }
            if ( !found )
            {
                return false;
            }
            else
                return true;
        }

        ILMethod CheckGenericParams ( ILMethod i, List<IType> param, IType [] genericArguments, ref bool match )
        {
            ILMethod genericMethod = null;
            if ( param != null )
            {
                for ( int j = 0; j < param.Count; j++ )
                {
                    var p = i.Parameters [ j ];
                    if ( p.IsGenericParameter )
                    {
                        if ( IsGenericArgumentMatch ( p, param [ j ], genericArguments ) )
                            continue;
                        else
                        {
                            match = false;
                            break;
                        }
                    }
                    if ( p.IsByRef )
                        p = p.ElementType;
                    if ( p.IsArray )
                        p = p.ElementType;

                    var p2 = param [ j ];
                    if ( p2.IsByRef )
                        p2 = p2.ElementType;
                    if ( p2.IsArray )
                        p2 = p2.ElementType;
                    if ( p.IsGenericParameter )
                    {
                        if ( i.Parameters [ j ].IsByRef == param [ j ].IsByRef && i.Parameters [ j ].IsArray == param [ j ].IsArray && IsGenericArgumentMatch ( p, p2, genericArguments ) )
                            continue;
                        else
                        {
                            match = false;
                            break;
                        }
                    }
                    if ( p.HasGenericParameter )
                    {
                        if ( p.Name != p2.Name )
                        {
                            match = false;
                            break;
                        }
                        //TODO should match the generic parameters;
                        continue;
                    }


                    if ( p2 != p )
                    {
                        match = false;
                        break;
                    }
                }
            }
            if ( match )
            {
                genericMethod = i;
            }
            return genericMethod;
        }

        public List<ILMethod> GetConstructors ()
        {
            if ( constructors == null )
                InitializeMethods ();
            return constructors;
        }

        public IMethod GetStaticConstroctor ()
        {
            if ( constructors == null )
                InitializeMethods ();
            return staticConstructor;
        }

        public IMethod GetConstructor ( int paramCnt )
        {
            if ( constructors == null )
                InitializeMethods ();
            foreach ( var i in constructors )
            {
                if ( i.ParameterCount == paramCnt )
                {
                    return i;
                }
            }
            return null;
        }
        public IMethod GetConstructor(List<IType> param)
        {
            return GetConstructor(param, true);
        }
        public IMethod GetConstructor(List<IType> param, bool exactMatch = true)
        {
            if (constructors == null)
                InitializeMethods();
            foreach (var i in constructors)
            {
                if (i.ParameterCount == param.Count)
                {
                    bool match = true;

                    for (int j = 0; j < param.Count; j++)
                    {
                        if ((exactMatch && param[j] != i.Parameters[j]) || !param[j].CanAssignTo(i.Parameters[j]))
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

        public int GetFieldIndex ( object token )
        {
            if ( fieldMapping == null )
                InitializeFields ();
            int idx;
            int hashCode = token.GetHashCode ();
            if ( fieldTokenMapping.TryGetValue ( hashCode, out idx ) )
                return idx;
            FieldReference f = token as FieldReference;
            if ( staticFieldMapping != null && staticFieldMapping.TryGetValue ( f.Name, out idx ) )
            {
                fieldTokenMapping [ hashCode ] = idx;
                return idx;
            }
            if ( fieldMapping.TryGetValue ( f.Name, out idx ) )
            {
                fieldTokenMapping [ hashCode ] = idx;
                return idx;
            }

            return -1;
        }

#if ENABLE_NEO_MODE
        internal ILTypeFieldOffset GetFieldOffset(object token)
        {
            var idx = GetFieldIndex(token);
            return GetFieldOffset(idx);
        }

        internal ILTypeFieldOffset GetFieldOffset(int idx)
        {
            if (idx < FieldStartIndex)
                return ((ILType)BaseType).GetFieldOffset(idx);
            else
            {
                return fieldOffsets[idx - FieldStartIndex];
            }
        }

        internal ILTypeFieldOffset GetStaticFieldOffset(int idx)
        {
            return staticFieldOffsets[idx];
        }
#endif

        public IType GetField(string name, out int fieldIdx)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (fieldMapping.TryGetValue(name, out fieldIdx))
            {
                return fieldTypes[fieldIdx - FieldStartIndex];
            }
            else if (BaseType != null && BaseType is ILType)
            {
                return ((ILType)BaseType).GetField(name, out fieldIdx);
            }
            else if (staticFieldMapping != null && staticFieldMapping.TryGetValue(name, out fieldIdx))
            {
                return staticFieldTypes[fieldIdx];
            }
            else 
                return null;
        }

        public IType GetField(int fieldIdx, out FieldReference fr)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (fieldIdx < FieldStartIndex)
                return ((ILType)BaseType).GetField(fieldIdx, out fr);
            else
            {
                fr = fieldReferences[fieldIdx - FieldStartIndex];
                return fieldTypes[fieldIdx - FieldStartIndex];
            }
        }

        public IType GetField(int fieldIdx, out FieldDefinition fd)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (fieldIdx < FieldStartIndex)
                return ((ILType)BaseType).GetField(fieldIdx, out fd);
            else
            {
                fd = fieldDefinitions[fieldIdx - FieldStartIndex];
                return fieldTypes[fieldIdx - FieldStartIndex];
            }
        }

        void InitializeFields ()
        {
#if ENABLE_NEO_MODE
            InitializeFieldsForFlatLayout();
#else
            InitializeFieldsForStackObjectLayout();
#endif
        }

#if !ENABLE_NEO_MODE
        void InitializeFieldsForStackObjectLayout ()
        {
            fieldMapping = new Dictionary<string, int> ();
            if (definition == null)
            {
                fieldTypes = new IType[0];
                fieldReferences = new FieldReference[0];
                fieldDefinitions = new FieldDefinition[0];
                return;
            }
            fieldTypes = new IType [ definition.Fields.Count ];
            fieldReferences = new FieldReference[definition.Fields.Count];
            fieldDefinitions = new FieldDefinition[definition.Fields.Count];
            var fields = definition.Fields;
            int idx = FieldStartIndex;
            int idxStatic = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.IsStatic)
                {
                    //It makes no sence to initialize
                    if (!TypeReference.HasGenericParameters || IsGenericInstance)
                    {
                        if (staticFieldTypes == null)
                        {
                            staticFieldTypes = new IType[definition.Fields.Count];
                            staticFieldReferences = new FieldReference[definition.Fields.Count];
                            staticFieldDefinitions = new FieldDefinition[definition.Fields.Count];
                            staticFieldMapping = new Dictionary<string, int>();
                        }
                        staticFieldMapping[field.Name] = idxStatic;
                        if (field.FieldType.IsGenericParameter)
                        {
                            staticFieldTypes[idxStatic] = FindGenericArgument(field.FieldType.Name);
                        }
                        else
                            staticFieldTypes[idxStatic] = appdomain.GetType(field.FieldType, this, null);
                        FieldReference fr = field;
                        if (typeRef.IsGenericInstance)
                        {
                            fr = new FieldReference(field.Name, staticFieldTypes[idxStatic].ToTypeReference(appdomain.LoadedModules[0]), typeRef);
                        }
                        staticFieldReferences[idxStatic] = fr;
                        staticFieldDefinitions[idxStatic] = field;
                        idxStatic++;
                    }
                }
                else
                {
                    fieldMapping[field.Name] = idx;
                    IType fieldType;
                    if (field.FieldType.IsGenericParameter)
                    {
                        fieldType = FindGenericArgument(field.FieldType.Name);
                    }
                    else
                        fieldType = appdomain.GetType(field.FieldType, this, null);
                    fieldTypes[idx - FieldStartIndex] = fieldType;
                    FieldReference fr = field;
                    if (typeRef.IsGenericInstance)
                    {
                        fr = new FieldReference(field.Name, fieldType.ToTypeReference(appdomain.LoadedModules[0]), typeRef);
                    }
                    fieldReferences[idx - FieldStartIndex] = fr;
                    fieldDefinitions[idx - FieldStartIndex] = field;
                    if (IsEnum)
                    {
                        enumType = fieldType;
                    }

                    idx++;
                }
            }
            Array.Resize ( ref fieldTypes, idx - FieldStartIndex );
            Array.Resize ( ref fieldDefinitions, idx - FieldStartIndex );

            if ( staticFieldTypes != null )
            {
                Array.Resize ( ref staticFieldTypes, idxStatic );
                Array.Resize ( ref staticFieldDefinitions, idxStatic );
                //staticInstance = new ILTypeStaticInstance(this);
            }
        }
#endif

#if ENABLE_NEO_MODE
        void InitializeFieldsForFlatLayout ()
        {
            fieldMapping = new Dictionary<string, int> ();
            if (definition == null)
            {
                fieldTypes = new IType[0];
                fieldReferences = new FieldReference[0];
                fieldDefinitions = new FieldDefinition[0];
                fieldOffsets = new ILTypeFieldOffset[0];
                staticFieldOffsets = null;
                refFieldPrimitiveOffsets = new int[0];
                totalPrimitiveSize = 0;
                totalReferenceCnt = 0;
                totalStaticPrimitiveSize = 0;
                totalStaticReferenceCnt = 0;
                return;
            }

            int AlignUp(int offset, int alignment)
            {
                return (offset + alignment - 1) & ~(alignment - 1);
            }

            int GetPrimitiveSizeFromClrType(Type t)
            {
                if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte))
                    return 1;
                if (t == typeof(short) || t == typeof(ushort) || t == typeof(char))
                    return 2;
                if (t == typeof(int) || t == typeof(uint) || t == typeof(float))
                    return 4;
                if (t == typeof(long) || t == typeof(ulong) || t == typeof(double) || t == typeof(IntPtr) || t == typeof(UIntPtr))
                    return 8;
                return 4;
            }

            int GetPrimitiveAlignmentFromClrType(Type t)
            {
                if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte))
                    return 1;
                if (t == typeof(short) || t == typeof(ushort) || t == typeof(char))
                    return 2;
                if (t == typeof(int) || t == typeof(uint) || t == typeof(float))
                    return 4;
                if (t == typeof(long) || t == typeof(ulong) || t == typeof(double) || t == typeof(IntPtr) || t == typeof(UIntPtr))
                    return 8;
                return 4;
            }

            int GetFieldNaturalSize(IType type)
            {
                if (type.IsPrimitive)
                {
                    return GetPrimitiveSizeFromClrType(type.TypeForCLR);
                }
                if (type.IsEnum)
                {
                    Type ut;
                    if (type is ILType ilEnum)
                    {
                        var _ = ilEnum.TotalFieldCount;
                        ut = ilEnum.enumType.TypeForCLR;
                    }
                    else
                    {
                        ut = type.TypeForCLR.GetEnumUnderlyingType();
                    }
                    return GetPrimitiveSizeFromClrType(ut);
                }
                if (type.IsValueType && type is ILType it)
                {
                    return it.TotalPrimitiveSize;
                }
                return 4;
            }

            int GetFieldNaturalAlignment(IType type)
            {
                if (type.IsPrimitive)
                {
                    return GetPrimitiveAlignmentFromClrType(type.TypeForCLR);
                }
                if (type.IsEnum)
                {
                    Type ut;
                    if (type is ILType ilEnum)
                    {
                        var _ = ilEnum.TotalFieldCount;
                        ut = ilEnum.enumType.TypeForCLR;
                    }
                    else
                    {
                        ut = type.TypeForCLR.GetEnumUnderlyingType();
                    }
                    return GetPrimitiveAlignmentFromClrType(ut);
                }
                if (type.IsValueType && type is ILType it)
                {
                    return GetStructMaxAlignment(it);
                }
                return 4;
            }

            int GetStructMaxAlignment(ILType type)
            {
                return GetStructMaxAlignmentCore(type, new HashSet<ILType>());
            }

            int GetStructMaxAlignmentCore(ILType type, HashSet<ILType> visited)
            {
                if (!visited.Add(type))
                    return 1;
                var _ = type.TotalFieldCount;
                int maxAlign = 1;
                if (type.BaseType != null && type.BaseType is ILType baseIl)
                {
                    int baseAlign = GetStructMaxAlignmentCore(baseIl, visited);
                    if (baseAlign > maxAlign)
                        maxAlign = baseAlign;
                }
                for (int i = 0; i < type.fieldTypes.Length; i++)
                {
                    int a = GetFieldNaturalAlignment(type.fieldTypes[i]);
                    if (a > maxAlign)
                        maxAlign = a;
                }
                return maxAlign;
            }

            fieldTypes = new IType [ definition.Fields.Count ];
            fieldOffsets = new ILTypeFieldOffset[definition.Fields.Count];
            staticFieldOffsets = new ILTypeFieldOffset[definition.Fields.Count];
            fieldReferences = new FieldReference[definition.Fields.Count];
            fieldDefinitions = new FieldDefinition[definition.Fields.Count];
            var fields = definition.Fields;
            int idx = FieldStartIndex;
            int idxStatic = 0;
            int primitiveOffset = 0;
            int referenceOffset = 0;
            int staticPrimitiveOffset = 0;
            int staticReferenceOffset = 0;
            List<int> refFieldOffsets = new List<int>();
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.IsStatic)
                {
                    if (!TypeReference.HasGenericParameters || IsGenericInstance)
                    {
                        if (staticFieldTypes == null)
                        {
                            staticFieldTypes = new IType[definition.Fields.Count];
                            staticFieldReferences = new FieldReference[definition.Fields.Count];
                            staticFieldDefinitions = new FieldDefinition[definition.Fields.Count];
                            staticFieldMapping = new Dictionary<string, int>();
                        }
                        staticFieldMapping[field.Name] = idxStatic;
                        IType staticFieldType;
                        if (field.FieldType.IsGenericParameter)
                        {
                            staticFieldType = FindGenericArgument(field.FieldType.Name);
                        }
                        else
                            staticFieldType = appdomain.GetType(field.FieldType, this, null);
                        staticFieldTypes[idxStatic] = staticFieldType;
                        FieldReference fr = field;
                        if (typeRef.IsGenericInstance)
                        {
                            fr = new FieldReference(field.Name, staticFieldType.ToTypeReference(appdomain.LoadedModules[0]), typeRef);
                        }
                        staticFieldReferences[idxStatic] = fr;
                        staticFieldDefinitions[idxStatic] = field;

                        int fSize = GetFieldNaturalSize(staticFieldType);
                        int fAlign = GetFieldNaturalAlignment(staticFieldType);
                        staticPrimitiveOffset = AlignUp(staticPrimitiveOffset, fAlign);
                        staticFieldOffsets[idxStatic] = new ILTypeFieldOffset()
                        {
                            PrimitiveOffset = staticPrimitiveOffset,
                            ReferenceOffset = staticReferenceOffset
                        };
                        staticPrimitiveOffset += fSize;
                        if (staticFieldType.IsPrimitive || staticFieldType.IsEnum)
                        {
                        }
                        else if (staticFieldType.IsValueType && staticFieldType is ILType sit)
                        {
                            staticReferenceOffset += sit.TotalReferenceCount;
                        }
                        else
                        {
                            staticReferenceOffset++;
                        }
                        idxStatic++;
                    }
                }
                else
                {
                    fieldMapping[field.Name] = idx;
                    IType fieldType;
                    if (field.FieldType.IsGenericParameter)
                    {
                        fieldType = FindGenericArgument(field.FieldType.Name);
                    }
                    else
                        fieldType = appdomain.GetType(field.FieldType, this, null);
                    fieldTypes[idx - FieldStartIndex] = fieldType;
                    FieldReference fr = field;
                    if (typeRef.IsGenericInstance)
                    {
                        fr = new FieldReference(field.Name, fieldType.ToTypeReference(appdomain.LoadedModules[0]), typeRef);
                    }
                    fieldReferences[idx - FieldStartIndex] = fr;
                    fieldDefinitions[idx - FieldStartIndex] = field;
                    if (IsEnum)
                    {
                        enumType = fieldType;
                    }

                    int fSize = GetFieldNaturalSize(fieldType);
                    int fAlign = GetFieldNaturalAlignment(fieldType);
                    primitiveOffset = AlignUp(primitiveOffset, fAlign);
                    fieldOffsets[idx - FieldStartIndex] = new ILTypeFieldOffset()
                    {
                        PrimitiveOffset = primitiveOffset,
                        ReferenceOffset = referenceOffset
                    };
                    bool fieldHasRefs = false;
                    if (fieldType.IsPrimitive || fieldType.IsEnum)
                    {
                        primitiveOffset += fSize;
                    }
                    else if (fieldType.IsValueType && fieldType is ILType it)
                    {
                        primitiveOffset += fSize;
                        referenceOffset += it.TotalReferenceCount;
                        if (it.TotalReferenceCount > 0)
                            fieldHasRefs = true;
                    }
                    else
                    {
                        primitiveOffset += fSize;
                        referenceOffset++;
                        fieldHasRefs = true;
                    }
                    if (fieldHasRefs)
                    {
                        refFieldOffsets.Add(primitiveOffset - fSize);
                    }
                    idx++;
                }
            }
            Array.Resize ( ref fieldTypes, idx - FieldStartIndex );
            Array.Resize ( ref fieldDefinitions, idx - FieldStartIndex );
            Array.Resize(ref fieldOffsets, idx - FieldStartIndex );

            int maxAlignment = GetStructMaxAlignment(this);
            totalPrimitiveSize = AlignUp(primitiveOffset, maxAlignment);
            if (totalPrimitiveSize < 1)
                totalPrimitiveSize = 1;
            totalReferenceCnt = referenceOffset;
            refFieldPrimitiveOffsets = refFieldOffsets.ToArray();

            if ( staticFieldTypes != null )
            {
                Array.Resize ( ref staticFieldTypes, idxStatic );
                Array.Resize ( ref staticFieldDefinitions, idxStatic );
                Array.Resize(ref staticFieldOffsets, idxStatic);
                int maxStaticAlignment = 1;
                for (int i = 0; i < staticFieldTypes.Length; i++)
                {
                    int a = GetFieldNaturalAlignment(staticFieldTypes[i]);
                    if (a > maxStaticAlignment)
                        maxStaticAlignment = a;
                }
                totalStaticPrimitiveSize = AlignUp(staticPrimitiveOffset, maxStaticAlignment);
                totalStaticReferenceCnt = staticReferenceOffset;
            }
            else
            {
                staticFieldOffsets = null;
                totalStaticPrimitiveSize = 0;
                totalStaticReferenceCnt = 0;
            }
        }
#endif

        public IType FindGenericArgument ( string key )
        {
            var o = this.Generic ( key );
            if ( o == null && definition.GenericParameters != null )
            {
                for ( int i = 0; i < definition.GenericParameters.Count; i++ )
                {
                    if ( definition.GenericParameters [ i ].Name == key )
                    {
                        return this.Generic ( "!" + i );
                    }
                }
            }
            return o;
        }

        private IType Generic ( string key )
        {
            if ( this.genericArguments != null )
            {
                for ( int i = 0; i < this.genericArguments.Length; i++ )
                {
                    if ( this.genericArguments [ i ].Key == key)
                    {
                        return this.genericArguments [ i ].Value;
                    }
                }
            }

            return null;
        }

        public bool CanAssignTo ( IType type )
        {
            bool res = false;
            if ( this == type )
            {
                return true;
            }

            if ( IsEnum )
            {
                if ( type.TypeForCLR == typeof ( Enum ) )
                    return true;
            }
            if ( BaseType != null )
            {
                res = BaseType.CanAssignTo ( type );

                if ( res ) return true;
            }

            if ( Implements != null )
            {
                for ( int i = 0; i < interfaces.Length; i++ )
                {
                    var im = interfaces [ i ];
                    res = im.CanAssignTo ( type );
                    if ( res )
                        return true;
                }
            }
            return res;
        }

        public ILTypeInstance Instantiate ( bool callDefaultConstructor = true )
        {
            var res = new ILTypeInstance ( this );
            if ( callDefaultConstructor )
            {
                var m = GetConstructor ( CLR.Utils.Extensions.EmptyParamList );
                if ( m != null )
                {
                    appdomain.Invoke ( m, res, null );
                }
            }
            return res;
        }

        public ILTypeInstance Instantiate(object[] args)
        {
            var res = new ILTypeInstance(this);
            var argsTypes = new List<IType>(args.Length);
            foreach (var o in args)
            {
                if (o is ILTypeInstance)
                {
                    argsTypes.Add(((ILTypeInstance)o).Type);
                }
                else
                {
                    argsTypes.Add(appdomain.GetType(o.GetType()));
                }
            }
            var m = GetConstructor(argsTypes, false);
            if (m != null)
            {
                appdomain.Invoke(m, res, args);
            }

            return res;
        }

        public IType MakeGenericInstance ( KeyValuePair<string, IType> [] genericArguments )
        {
            if ( genericInstances == null )
                genericInstances = new List<ILType> ();
            foreach ( var i in genericInstances )
            {
                bool match = true;
                for ( int j = 0; j < genericArguments.Length; j++ )
                {
                    if (i.genericArguments[j].Value is ILGenericParameterType ptA && genericArguments[j].Value is ILGenericParameterType ptB)
                    {
                        if(ptA.TypeReference != ptB.TypeReference)
                        {
                            match = false;
                            break;
                        }
                    }
                    else if ( i.genericArguments [ j ].Value != genericArguments [ j ].Value )
                    {
                        match = false;
                        break;
                    }
                }
                if ( match )
                    return i;
            }
            GenericInstanceType def = new GenericInstanceType(typeRef);
            foreach (var i in genericArguments)
            {
                TypeReference tRef = null;
                if (i.Value is ILType ilType)
                {
                    tRef = ilType.typeRef;
                }
                else if (i.Value is ILGenericParameterType gpt)
                {
                    tRef = gpt.TypeReference;
                }
                else
                {
                    CLRType clrType = (CLRType)i.Value;
                    tRef = appdomain.LoadedModules[0].ImportReference(clrType.TypeForCLR);
                }
                def.GenericArguments.Add(tRef);
            }
            var res = new ILType ( def, appdomain );
            res.genericDefinition = this;
            res.genericArguments = genericArguments;
            foreach (var i in genericArguments)
            {
                if (i.Value.HasGenericParameter)
                {
                    res.hasGenericArguments = true;
                    break;
                }
            }
            genericInstances.Add ( res );
            return res;
        }

        public IType MakeByRefType ()
        {
            if ( byRefType == null )
            {
                var def = new ByReferenceType ( typeRef );
                byRefType = new ILType ( def, appdomain );
                ( ( ILType ) byRefType ).elementType = this;
                ( ( ILType ) byRefType ).byRefCLRType = this.TypeForCLR.MakeByRefType ();
            }
            return byRefType;
        }

        public IType MakeArrayType ( int rank )
        {
            if ( arrayTypes == null )
                arrayTypes = new Dictionary<int, IType> ();
            IType atype;
            if ( !arrayTypes.TryGetValue ( rank, out atype ) )
            {
                var def = new ArrayType ( typeRef, rank );
                atype = new ILType ( def, appdomain );
                ( ( ILType ) atype ).IsArray = true;
                ( ( ILType ) atype ).elementType = this;
                ( ( ILType ) atype ).arrayCLRType = rank > 1 ? this.TypeForCLR.MakeArrayType ( rank ) : this.TypeForCLR.MakeArrayType ();
                arrayTypes [ rank ] = atype;
            }
            return atype;
        }

        public IType ResolveGenericType ( IType contextType )
        {
            var ga = contextType.GenericArguments;
            if ( definition == null )
                return null;
            IType [] kv = new IType [ definition.GenericParameters.Count ];
            for ( int i = 0; i < kv.Length; i++ )
            {
                var gp = definition.GenericParameters [ i ];
                string name = gp.Name;
                foreach ( var j in ga )
                {
                    if ( j.Key == name )
                    {
                        kv [ i ] = j.Value;
                        break;
                    }
                }
            }

            foreach ( var i in genericInstances )
            {
                bool match = true;
                for ( int j = 0; j < kv.Length; j++ )
                {
                    if ( i.genericArguments [ j ].Value != kv [ j ] )
                    {
                        match = false;
                        break;
                    }
                }
                if ( match )
                    return i;
            }

            return null;
        }

        public int GetStaticFieldSizeInMemory ( HashSet<object> traversed )
        {
            return staticInstance != null ? staticInstance.GetSizeInMemory ( traversed ) : 0;
        }

        public unsafe int GetMethodBodySizeInMemory ()
        {
            int size = 0;
            if ( methods != null )
            {
                foreach ( var i in methods )
                {
                    foreach ( var j in i.Value )
                    {
                        if ( j.HasBody )
                        {
                            size += j.Body.Length * sizeof ( Runtime.Intepreter.OpCodes.OpCode );
                        }
                    }
                }
            }
            return size;
        }

        public ValueTypeInitInfo ValueTypeInitializationInfo
        {
            get
            {
                if(vtInitInfo == null)
                {
                    if (IsValueType)
                        vtInitInfo = new ValueTypeInitInfo(this);
                }
                return vtInitInfo;
            }
        }

        public void GetValueTypeSize ( out int fieldCout, out int managedCount )
        {
            if ( !valuetypeSizeCalculated )
            {
                valuetypeFieldCount = FieldTypes.Length + 1;
                valuetypeManagedCount = 0;
                for ( int i = 0; i < FieldTypes.Length; i++ )
                {
                    var ft = FieldTypes [ i ];
                    if ( ft.IsValueType )
                    {
                        if ( !ft.IsPrimitive && !ft.IsEnum )
                        {
                            if ( ft is ILType || ( ( CLRType ) ft ).ValueTypeBinder != null )
                            {
                                int fSize, fmCnt;
                                ft.GetValueTypeSize ( out fSize, out fmCnt );
                                valuetypeFieldCount += fSize;
                                valuetypeManagedCount += fmCnt;
                            }
                            else
                            {
                                valuetypeManagedCount++;
                            }
                        }
                    }
                    else
                    {
                        valuetypeManagedCount++;
                    }
                }
                if ( BaseType != null && BaseType is ILType )
                {
                    int fSize, fmCnt;
                    BaseType.GetValueTypeSize ( out fSize, out fmCnt );
                    valuetypeFieldCount += fSize - 1;//no header for base type fields
                    valuetypeManagedCount += fmCnt;
                }
                valuetypeSizeCalculated = true;
            }
            fieldCout = valuetypeFieldCount;
            managedCount = valuetypeManagedCount;
        }

        public override int GetHashCode ()
        {
            if ( hashCode == -1 )
                hashCode = System.Threading.Interlocked.Add ( ref instance_id, 1 );
            return hashCode;
        }

        public override string ToString ()
        {
            return FullName;
        }
    }
}
