using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

using ILRuntime.Mono.Cecil;
using ILRuntime.CLR.Method;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.Hybrid;


#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.CLR.TypeSystem
{
    public sealed unsafe class CLRType : IType
    {
        Type clrType;
        bool isPrimitive, isValueType, isEnum;
        Dictionary<string, List<CLRMethod>> methods;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        List<CLRMethod> constructors;
        KeyValuePair<string, IType>[] genericArguments;
        List<CLRType> genericInstances;
        Dictionary<string, int> fieldMapping;
        Dictionary<int, FieldInfo> fieldInfoCache;
        Dictionary<int, CLRFieldGetterDelegate> fieldGetterCache;
        Dictionary<int, CLRFieldSetterDelegate> fieldSetterCache;
        Dictionary<int, KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>> fieldBindingCache;
        Dictionary<int, KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate>> fieldNeoBindingCache;
        StackObject defaultObject;

        Dictionary<int, int> fieldIdxMapping;
        IType[] orderedFieldTypes;

        CLRMemberwiseCloneDelegate memberwiseCloneDelegate;
        CLRCreateDefaultInstanceDelegate createDefaultInstanceDelegate;
        CLRCreateArrayInstanceDelegate createArrayInstanceDelegate;
        Dictionary<int, int> fieldTokenMapping;
        IType byRefType, elementType;
        Dictionary<int, IType> arrayTypes;
        IType[] interfaces;
        bool isDelegate;
        IType baseType;
        bool isBaseTypeInitialized = false, interfaceInitialized = false, valueTypeBinderGot = false;
        ILRuntimeWrapperType wraperType;
        ValueTypeBinder valueTypeBinder;

        int valuetypeFieldCount, valuetypeManagedCount;
        bool valuetypeSizeCalculated;
        ValueTypeInitInfo vtInitInfo;
        int hashCode = -1;
        int tIdx = -1;
        static int instance_id = 0x20000000;

#if ENABLE_NEO_MODE
        // Neo storage classification: cached on first InitializeFields() call.
        StructStorage structStorage;
        int totalPrimitiveSize;
        int totalReferenceCount;
        // fieldHash -> (primitive byte offset, reference slot index, declared field type)
        Dictionary<int, ILTypeFieldOffset> neoFieldOffsets;
        // Per-field setter/getter delegates for reference fields in Inline layout (indexed by ref slot order).
        FieldInfo[] inlineRefFieldInfos;
        internal int MaxAlignment { get; private set; }

        /// <summary>
        /// Compile-time storage classification of this type. See <see cref="TypeSystem.StructStorage"/>.
        /// Only valid after <see cref="InitializeFields"/> runs (accessing this property will trigger it).
        /// </summary>
        public StructStorage StructStorage
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return structStorage;
            }
        }

        /// <summary>
        /// Total primitive byte size of this value type's Inline layout. Returns 4 (mStack index size) for
        /// Boxed and NotValueType classifications.
        /// </summary>
        public int TotalPrimitiveSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return totalPrimitiveSize;
            }
        }

        /// <summary>
        /// Total number of mStack reference slots consumed by this value type's Inline layout. Returns 1 for
        /// Boxed and NotValueType (single mStack slot holding the boxed CLR object or reference).
        /// </summary>
        public int TotalReferenceCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return totalReferenceCount;
            }
        }

        /// <summary>
        /// Returns the primitive byte offset of the given field within this type's Inline layout,
        /// or -1 if the field is unknown or this type is not Inline.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFieldPrimitiveOffset(int fieldHash)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (neoFieldOffsets != null && neoFieldOffsets.TryGetValue(fieldHash, out var off))
                return off.PrimitiveOffset;
            return -1;
        }

        /// <summary>
        /// Returns the mStack reference slot index of the given field within this type's Inline layout,
        /// or -1 if the field is unknown, is a pure primitive/enum field with no ref segment, or this type is not Inline.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFieldReferenceOffset(int fieldHash)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (neoFieldOffsets != null && neoFieldOffsets.TryGetValue(fieldHash, out var off))
                return off.ReferenceOffset;
            return -1;
        }

        /// <summary>
        /// Returns the FieldInfo for the given reference-segment slot index within this Inline-layout struct.
        /// Slot index 0..TotalReferenceCount-1 addresses fields in the same order they appear in the
        /// ref segment; used by Box / Unbox to copy reference fields between mStack and the boxed object.
        /// Returns null when index is out of range or this type is not Inline.
        /// </summary>
        internal FieldInfo GetInlineRefFieldInfo(int refIndex)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (inlineRefFieldInfos == null || refIndex < 0 || refIndex >= inlineRefFieldInfos.Length)
                return null;
            return inlineRefFieldInfos[refIndex];
        }
#endif

        public Dictionary<int, FieldInfo> Fields
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return fieldInfoCache;
            }
        }

        public Dictionary<int, int> FieldIndexMapping
        {
            get { return fieldIdxMapping; }
        }

        public IType[] OrderedFieldTypes
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();
                return orderedFieldTypes;
            }
        }

        public int TotalFieldCount
        {
            get
            {
                if (fieldMapping == null)
                    InitializeFields();

                if (fieldIdxMapping != null)
                    return fieldIdxMapping.Count;
                else
                    throw new NotSupportedException("Cannot find ValueTypeBinder for type:" + clrType.FullName);
            }
        }

        internal PatchGetFieldDelegate GetStaticFieldCallback { get; set; }
        internal PatchSetFieldDelegate SetStaticFieldCallback { get; set; }

        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        public CLRType(Type clrType, Runtime.Enviorment.AppDomain appdomain)
        {
            this.clrType = clrType;
            this.appdomain = appdomain;
            isPrimitive = clrType.IsPrimitive;
            isEnum = clrType.IsEnum;
            isValueType = clrType.IsValueType;
            isDelegate = clrType.BaseType == typeof(MulticastDelegate) || clrType == typeof(Delegate);
            if (isPrimitive)
            {
                var t = TypeForCLR;
                if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) || t == typeof(bool))
                {
                    defaultObject.ObjectType = ObjectTypes.Integer;
                    defaultObject.Value = 0;
                    defaultObject.ValueLow = 0;
                }
                else if (t == typeof(long) || t == typeof(ulong))
                {
                    defaultObject.ObjectType = ObjectTypes.Long;
                    defaultObject.Value = 0;
                    defaultObject.ValueLow = 0;
                }
                else if (t == typeof(float))
                {
                    defaultObject.ObjectType = ObjectTypes.Float;
                    defaultObject.Value = 0;
                    defaultObject.ValueLow = 0;
                }
                else if (t == typeof(double))
                {
                    defaultObject.ObjectType = ObjectTypes.Double;
                    defaultObject.Value = 0;
                    defaultObject.ValueLow = 0;
                }
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

        public IType ElementType { get { return elementType; } }

        public bool HasGenericParameter
        {
            get
            {
                if (genericArguments != null)
                {
                    foreach(var i in genericArguments)
                    {
                        if (i.Value is ILType && i.Value.HasGenericParameter)
                        {
                            return true;
                        }
                        else if (i.Value is ILGenericParameterType)
                            return true;
                        else if(i.Value.HasGenericParameter)
                        {
                            return true;
                        } 
                    }
                }
                return clrType.ContainsGenericParameters;
            }
        }

        public bool IsGenericParameter
        {
            get
            {
                return clrType.IsGenericParameter;
            }
        }

        public bool IsInterface
        {
            get { return clrType.IsInterface; }
        }

        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }

        public Type ReflectionType
        {
            get
            {
                if (wraperType == null)
                    wraperType = new ILRuntimeWrapperType(this);
                return wraperType;
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
                return arrayTypes != null ? arrayTypes[1] : null;
            }
        }

        public bool IsArray
        {
            get;private set;
        }

        public int ArrayRank
        {
            get;private set;
        }

        public bool IsValueType
        {
            get
            {
                return isValueType;
            }
        }

        public bool IsByRef
        {
            get
            {
                return clrType.IsByRef;
            }
        }

        public bool IsDelegate
        {
            get
            {
                return isDelegate;
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return isPrimitive;
            }
        }

        public bool IsEnum
        {
            get
            {
                return isEnum;
            }
        }
        public string FullName
        {
            get
            {
                return clrType.FullName;
            }
        }
        public string Name
        {
            get
            {
                return clrType.Name;
            }
        }

        public IType BaseType
        {
            get
            {
                if (!isBaseTypeInitialized)
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

        public ValueTypeBinder ValueTypeBinder
        {
            get
            {
                if (clrType.IsValueType)
                {
                    if (!valueTypeBinderGot)
                    {
                        valueTypeBinderGot = true;
                        appdomain.ValueTypeBinders.TryGetValue(clrType, out valueTypeBinder);
                    }
                    return valueTypeBinder;
                }
                else
                    return null;
            }
        }

        public StackObject DefaultObject
        {
            get
            {
                return defaultObject;
            }
        }

        public int TypeIndex
        {
            get
            {
                if (tIdx < 0)
                    tIdx = appdomain.AllocTypeIndex(this);
                return tIdx;
            }
        }

        public object PerformMemberwiseClone(object target)
        {
            if (memberwiseCloneDelegate == null)
            {
                if (!AppDomain.MemberwiseCloneMap.TryGetValue(this.clrType, out memberwiseCloneDelegate))
                {
                    var memberwiseClone = clrType.GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (memberwiseClone != null)
                    {
                        var del = (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), memberwiseClone);
                        memberwiseCloneDelegate = (ref object t) => del(t);
                    }
                    else
                    {
                        throw new InvalidOperationException("Memberwise clone method not found for " + clrType.FullName);
                    }
                }
            }

            return memberwiseCloneDelegate(ref target);
        }

        void InitializeBaseType()
        {
            baseType = appdomain.GetType(clrType.BaseType);
            if (baseType.TypeForCLR == typeof(Enum) || baseType.TypeForCLR == typeof(object) || baseType.TypeForCLR == typeof(ValueType) || baseType.TypeForCLR == typeof(System.Enum))
            {//都是这样，无所谓
                baseType = null;
            }
            isBaseTypeInitialized = true;
        }

        void InitializeInterfaces()
        {
            interfaceInitialized = true;
            var arr = clrType.GetInterfaces();
            if (arr.Length >0)
            {
                interfaces = new IType[arr.Length];
                for (int i = 0; i < interfaces.Length; i++)
                {
                    interfaces[i] = appdomain.GetType(arr[i]);
                }
            }
        }

        public object GetFieldValue(int hash, object target)
        {
            if (fieldMapping == null)
                InitializeFields();

            var getter = GetFieldGetter(hash);
            if (getter != null)
            {
                return getter(ref target);
            }

            var fieldinfo = GetField(hash);
            if (fieldinfo != null)
            {
#if ENABLE_NEO_MODE
                if (structStorage == StructStorage.Inline && isValueType &&
                    target != null && !fieldinfo.FieldType.IsValueType)
                {
                    int primOff = GetFieldPrimitiveOffset(hash);
                    int refOff = GetFieldReferenceOffset(hash);
                    if (primOff >= 0 && refOff >= 0)
                    {
                        ref byte payload = ref Unsafe.As<RawObjectPayload>(target).Data;
                        return Unsafe.As<byte, object>(ref Unsafe.Add(ref payload, primOff));
                    }
                }
#endif
                return fieldinfo.GetValue(target);
            }

            return null;
        }

        public bool CopyFieldToStack(int hash, object target, Runtime.Intepreter.ILIntepreter intp, ref StackObject* esp, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var binding = GetFieldBinding(hash);
            if (binding.Key != null)
            {
                esp = binding.Key(ref target, intp, esp, mStack);
                return true;
            }
            else
                return false;
        }

        public bool AssignFieldFromStack(int hash, ref object target, Runtime.Intepreter.ILIntepreter intp, StackObject* esp, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var binding = GetFieldBinding(hash);
            if (binding.Value != null)
            {
                esp = binding.Value(ref target, intp, esp, mStack);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Read a CLR field into Neo's byte* frame layout: primitive fields fill their native width
        /// (sub-int extended to a 4-byte slot), reference fields write into mStack[dstRefBase] and
        /// store `dstRefBase` (or -1 for null) into the 4-byte primitive slot.
        /// Prefers the registered <see cref="CLRFieldNeoGetterDelegate"/> (zero boxing).
        /// Falls back to reflection + boxing dispatch by field type when no binding is registered.
        /// </summary>
#if ENABLE_NEO_MODE
        public unsafe void CopyFieldToNeoFrame(int hash, object target, Runtime.Intepreter.ILIntepreter intp, byte* dst, int dstRefBase, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var neoBinding = GetFieldNeoBinding(hash);
            if (neoBinding.Key != null)
            {
                neoBinding.Key(ref target, intp, dst, dstRefBase, mStack);
                return;
            }

            var fi = GetField(hash);
            if (fi == null)
                throw new MissingFieldException($"CLR field 0x{hash:X8} not found on {TypeForCLR}");

            if (structStorage == StructStorage.Inline && isValueType &&
                target != null && !fi.FieldType.IsValueType)
            {
                int primOff = GetFieldPrimitiveOffset(hash);
                int refOff = GetFieldReferenceOffset(hash);
                if (primOff >= 0 && refOff >= 0)
                {
                    ref byte payload = ref Unsafe.As<RawObjectPayload>(target).Data;
                    object value = Unsafe.As<byte, object>(
                        ref Unsafe.Add(ref payload, primOff));
                    mStack[dstRefBase] = value;
                    *(int*)dst = value != null ? dstRefBase : -1;
                    return;
                }
            }

            if (structStorage == StructStorage.Inline && isValueType &&
                target != null && (fi.FieldType.IsPrimitive || fi.FieldType.IsEnum ||
                                   fi.FieldType == typeof(IntPtr) || fi.FieldType == typeof(UIntPtr)))
            {
                int primOff = GetFieldPrimitiveOffset(hash);
                if (primOff >= 0)
                {
                    ref byte payload = ref Unsafe.As<RawObjectPayload>(target).Data;
                    MemoryLayoutHelpers.CopyPrimitiveToNeoFrame(
                        ref Unsafe.Add(ref payload, primOff), dst, fi.FieldType);
                    return;
                }
            }

            CopyValueToNeoFrame(fi.FieldType, fi.GetValue(target), dst, dstRefBase, mStack);
        }


        /// <summary>
        /// Write a CLR field from Neo's byte* frame layout, prefers the registered
        /// <see cref="CLRFieldNeoSetterDelegate"/> (zero boxing) and falls back to reflection.
        /// </summary>
        public unsafe void AssignFieldFromNeoFrame(int hash, ref object target, Runtime.Intepreter.ILIntepreter intp, byte* src, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var neoBinding = GetFieldNeoBinding(hash);
            if (neoBinding.Value != null)
            {
                neoBinding.Value(ref target, intp, src, mStack);
                return;
            }

            var fi = GetField(hash);
            if (fi == null)
                throw new MissingFieldException($"CLR field 0x{hash:X8} not found on {TypeForCLR}");

            if (structStorage == StructStorage.Inline && isValueType &&
                target != null && (fi.FieldType.IsPrimitive || fi.FieldType.IsEnum ||
                                   fi.FieldType == typeof(IntPtr) || fi.FieldType == typeof(UIntPtr)))
            {
                int primOff = GetFieldPrimitiveOffset(hash);
                if (primOff >= 0)
                {
                    ref byte payload = ref Unsafe.As<RawObjectPayload>(target).Data;
                    MemoryLayoutHelpers.CopyPrimitiveFromNeoFrame(
                        src, ref Unsafe.Add(ref payload, primOff), fi.FieldType);
                    return;
                }
            }

            if (structStorage == StructStorage.Inline && isValueType &&
                target != null && !fi.FieldType.IsValueType)
            {
                int primOff = GetFieldPrimitiveOffset(hash);
                int refOff = GetFieldReferenceOffset(hash);
                if (primOff >= 0 && refOff >= 0)
                {
                    int srcIdx = *(int*)src;
                    object refValue = srcIdx >= 0 ? mStack[srcIdx] : null;
                    ref byte payload = ref Unsafe.As<RawObjectPayload>(target).Data;
                    Unsafe.As<byte, object>(
                        ref Unsafe.Add(ref payload, primOff)) = refValue;
                    return;
                }
            }

            object value = ReadValueFromNeoFrame(fi.FieldType, src, mStack);
            fi.SetValue(target, value);
        }

        /// <summary>
        /// Static-field variants of <see cref="CopyFieldToNeoFrame"/> / <see cref="AssignFieldFromNeoFrame"/>.
        /// The bindings themselves ignore the target argument for static fields; the reflection
        /// fallback passes null.
        /// </summary>
        public unsafe void CopyStaticFieldToNeoFrame(int hash, Runtime.Intepreter.ILIntepreter intp, byte* dst, int dstRefBase, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var neoBinding = GetFieldNeoBinding(hash);
            if (neoBinding.Key != null)
            {
                object target = null;
                neoBinding.Key(ref target, intp, dst, dstRefBase, mStack);
                return;
            }

            var fi = GetField(hash);
            if (fi == null)
                throw new MissingFieldException($"CLR static field 0x{hash:X8} not found on {TypeForCLR}");
            CopyValueToNeoFrame(fi.FieldType, fi.GetValue(null), dst, dstRefBase, mStack);
        }

        public unsafe void AssignStaticFieldFromNeoFrame(int hash, Runtime.Intepreter.ILIntepreter intp, byte* src, AutoList mStack)
        {
            if (fieldMapping == null)
                InitializeFields();
            var neoBinding = GetFieldNeoBinding(hash);
            if (neoBinding.Value != null)
            {
                object target = null;
                neoBinding.Value(ref target, intp, src, mStack);
                return;
            }

            var fi = GetField(hash);
            if (fi == null)
                throw new MissingFieldException($"CLR static field 0x{hash:X8} not found on {TypeForCLR}");
            object value = ReadValueFromNeoFrame(fi.FieldType, src, mStack);
            fi.SetValue(null, value);
        }

        unsafe void CopyValueToNeoFrame(Type ft, object value, byte* dst, int dstRefBase, AutoList mStack)
        {
            if (ft.IsPrimitive || ft.IsEnum || ft == typeof(IntPtr) || ft == typeof(UIntPtr))
            {
                // Reuse the shared Neo primitive marshaller (mirror of WriteNeoPrimitive used by
                // InvocationFrame / CLR-return paths). Widening + enum semantics live there.
                var it = appdomain.GetType(ft);
                Runtime.Intepreter.ILIntepreter.WriteNeoPrimitive(dst, it, value);
                return;
            }
            if (ft.IsValueType)
            {
                // CLR value type field. Dispatch by StructStorage.
                var fieldClrType = appdomain.GetType(ft) as CLRType;
                if (fieldClrType == null)
                    throw new NotSupportedException($"Neo CopyFieldToNeoFrame: unresolved CLR value type '{ft.FullName}'.");
                if (fieldClrType.StructStorage == StructStorage.Inline)
                {
                    // Frame-uniform Inline copy via reflection-driven per-field walk.
                    Runtime.Intepreter.ILIntepreter.CopyBoxedClrObjectToFrameStatic(value, fieldClrType, dst, mStack, dstRefBase);
                }
                else
                {
                    // Boxed CLR value type: store the boxed reference in mStack, primitive slot holds mStack index.
                    if (value != null)
                    {
                        mStack[dstRefBase] = value;
                        *(int*)dst = dstRefBase;
                    }
                    else
                    {
                        mStack[dstRefBase] = null;
                        *(int*)dst = -1;
                    }
                }
                return;
            }

            // Reference type: reserve a stable slot and record either the index or -1 sentinel.
            if (value != null)
            {
                mStack[dstRefBase] = value;
                *(int*)dst = dstRefBase;
            }
            else
            {
                mStack[dstRefBase] = null;
                *(int*)dst = -1;
            }
        }

        unsafe object ReadValueFromNeoFrame(Type ft, byte* src, AutoList mStack)
        {
            if (ft.IsPrimitive || ft.IsEnum || ft == typeof(IntPtr) || ft == typeof(UIntPtr))
            {
                var it = appdomain.GetType(ft);
                return Runtime.Intepreter.ILIntepreter.ReadNeoPrimitive(src, it);
            }
            if (ft.IsValueType)
            {
                var fieldClrType = appdomain.GetType(ft) as CLRType;
                if (fieldClrType == null)
                    throw new NotSupportedException($"Neo AssignFieldFromNeoFrame: unresolved CLR value type '{ft.FullName}'.");
                if (fieldClrType.StructStorage == StructStorage.Inline)
                {
                    var boxed = fieldClrType.CreateDefaultInstance();
                    // Frame ref base is not passed through this API; caller-side callers using
                    // ReadValueFromNeoFrame for Inline structs must ensure refBase alignment matches
                    // the read source. Currently only reference-type fallback exercises this path;
                    // Inline value-type read requires a separate refBase argument (Step 15 binding generator).
                    // For now, materialize the boxed object using field-by-field walk with mStack ref segment
                    // implied to start at slot 0 (only correct when the source frame has no ref fields).
                    Runtime.Intepreter.ILIntepreter.CopyFrameToBoxedClrObjectStatic(boxed, fieldClrType, src, mStack, 0);
                    return boxed;
                }
                int refIdx = *(int*)src;
                return refIdx < 0 ? null : mStack[refIdx];
            }

            int idx = *(int*)src;
            return idx < 0 ? null : mStack[idx];
        }
#endif

        public void SetStaticFieldValue(int hash, object value)
        {
            if (fieldMapping == null)
                InitializeFields();

            var setter = GetFieldSetter(hash);
            object target = null;
            if (setter != null)
            {
                setter(ref target, value);
                return;
            }

            var fieldInfo = GetField(hash);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(null, value);
            }
        }

        public unsafe void SetFieldValue(int hash, ref object target, object value, bool directSet = false)
        {
            if (fieldMapping == null)
                InitializeFields();

            if (!directSet)
            {
                var setter = GetFieldSetter(hash);
                if (setter != null)
                {
                    setter(ref target, value);
                    return;
                }
            }
            var fieldInfo = GetField(hash);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
            }
        }

        KeyValuePair<CLRFieldBindingDelegate,CLRFieldBindingDelegate> GetFieldBinding(int hash)
        {
            var dic = fieldBindingCache;
            KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate> res;
            if (dic != null && dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetFieldBinding(hash);
            else
                return default(KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>);
        }

        KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate> GetFieldNeoBinding(int hash)
        {
            var dic = fieldNeoBindingCache;
            KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate> res;
            if (dic != null && dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetFieldNeoBinding(hash);
            else
                return default(KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate>);
        }

        private CLRFieldGetterDelegate GetFieldGetter(int hash)
        {
            var dic = fieldGetterCache;
            CLRFieldGetterDelegate res;
            if (dic != null && dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetFieldGetter(hash);
            else
                return null;
        }

        private CLRFieldSetterDelegate GetFieldSetter(int hash)
        {
            var dic = fieldSetterCache;
            CLRFieldSetterDelegate res;
            if (dic != null && dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetFieldSetter(hash);
            else
                return null;
        }

        public FieldInfo GetField(int hash)
        {
            var dic = Fields;
            FieldInfo res;
            if (dic.TryGetValue(hash, out res))
                return res;
            else if (BaseType != null)
                return ((CLRType)BaseType).GetField(hash);
            else
                return null;
        }

        void InitializeMethods()
        {
            methods = new Dictionary<string, List<CLRMethod>>();
            constructors = new List<CLRMethod>();
            foreach (var i in clrType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (i.IsPrivate)
                    continue;
                List<CLRMethod> lst;
                if (!methods.TryGetValue(i.Name, out lst))
                {
                    lst = new List<CLRMethod>();
                    methods[i.Name] = lst;
                }
                lst.Add(new CLRMethod(i, this, appdomain));
            }
            foreach (var i in clrType.GetConstructors())
            {
                constructors.Add(new CLRMethod(i, this, appdomain));
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

        public IMethod GetVirtualMethod(IMethod method)
        {
            var m = GetMethod(method.Name, method.Parameters, null, method.ReturnType);
            if (m == null)
            {
                return method;
            }
            else
                return m;
        }

        void InitializeFields()
        {
            fieldMapping = new Dictionary<string, int>();
            fieldInfoCache = new Dictionary<int, FieldInfo>();

            var fields = clrType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static).ToList();
            int idx = 0;
            bool hasValueTypeBinder = ValueTypeBinder != null;
            bool forceIncludePrivate = hasValueTypeBinder || clrType.IsDefined(typeof(Other.ILRuntimeBlittableAttribute), false);
            if (hasValueTypeBinder)
            {
                fieldIdxMapping = new Dictionary<int, int>();
            }
            if (hasValueTypeBinder || isEnum)
            {
                orderedFieldTypes = new IType[fields.Count];
            }

            fields.Sort((a, b) =>
            {
                return a.MetadataToken - b.MetadataToken;
            });
            foreach (var i in fields)
            {
                int hashCode = i.GetHashCode();

                if (i.IsPublic || i.IsFamily || forceIncludePrivate)
                {
                    fieldMapping[i.Name] = hashCode;
                    fieldInfoCache[hashCode] = i;
                }
                if ((hasValueTypeBinder || isEnum) && !i.IsStatic)
                {
                    orderedFieldTypes[idx] = appdomain.GetType(i.FieldType);
                    if (hasValueTypeBinder)
                        fieldIdxMapping[hashCode] = idx++;
                    else
                        idx++;
                }

                CLRFieldGetterDelegate getter;
                if (AppDomain.FieldGetterMap.TryGetValue(i, out getter))
                {
                    if (fieldGetterCache == null) fieldGetterCache = new Dictionary<int, CLRFieldGetterDelegate>();
                    fieldGetterCache[hashCode] = getter;
                }

                CLRFieldSetterDelegate setter;
                if (AppDomain.FieldSetterMap.TryGetValue(i, out setter))
                {
                    if (fieldSetterCache == null) fieldSetterCache = new Dictionary<int, CLRFieldSetterDelegate>();
                    fieldSetterCache[hashCode] = setter;
                }

                KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate> binding;
                if(AppDomain.FieldBindingMap.TryGetValue(i, out binding))
                {
                    if (fieldBindingCache == null) fieldBindingCache = new Dictionary<int, KeyValuePair<CLRFieldBindingDelegate, CLRFieldBindingDelegate>>();
                    fieldBindingCache[hashCode] = binding;
                }

                KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate> neoBinding;
                if (AppDomain.FieldNeoBindingMap.TryGetValue(i, out neoBinding))
                {
                    if (fieldNeoBindingCache == null) fieldNeoBindingCache = new Dictionary<int, KeyValuePair<CLRFieldNeoGetterDelegate, CLRFieldNeoSetterDelegate>>();
                    fieldNeoBindingCache[hashCode] = neoBinding;
                }
            }
            if (orderedFieldTypes != null)
            {
                Array.Resize(ref orderedFieldTypes, idx);
            }

#if ENABLE_NEO_MODE
            ClassifyStructStorageAndBuildLayout();
#endif
        }

#if ENABLE_NEO_MODE
        void ClassifyStructStorageAndBuildLayout()
        {
            if (!isValueType)
            {
                structStorage = StructStorage.NotValueType;
                totalPrimitiveSize = 4;
                totalReferenceCount = 1;
                return;
            }

            bool inline = ValueTypeBinder != null
                          || clrType.IsDefined(typeof(Other.ILRuntimeBlittableAttribute), false)
                          || (!HasInstanceMethods() && !HasPrivateFields());

            if (!inline)
            {
                structStorage = StructStorage.Boxed;
                totalPrimitiveSize = 4;
                totalReferenceCount = 1;
                return;
            }

            structStorage = StructStorage.Inline;

            // Build flat layout for CLR value type. Mirrors ILType.InitializeFieldsForFlatLayout.
            var declaredFields = clrType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Array.Sort(declaredFields, (a, b) => a.MetadataToken - b.MetadataToken);
            neoFieldOffsets = new Dictionary<int, ILTypeFieldOffset>();
            var refFieldInfos = new List<FieldInfo>();
            int primitiveOffset = 0;
            int referenceOffset = 0;
            int maxAlignment = 1;

            foreach (var fi in declaredFields)
            {
                var ft = fi.FieldType;
                int fSize = MemoryLayoutHelpers.GetPrimitiveSizeFromClrType(ft);
                int fAlign = MemoryLayoutHelpers.GetPrimitiveAlignmentFromClrType(ft);
                bool nestedInlineStruct = false;
                CLRType nestedClrType = null;
                ILType nestedIlType = null;
                if (!ft.IsPrimitive && !ft.IsEnum && ft != typeof(IntPtr) && ft != typeof(UIntPtr))
                {
                    var nestedType = appdomain.GetType(ft);
                    if (nestedType is ILType nt && nt.IsValueType)
                    {
                        nestedIlType = nt;
                        fSize = nt.TotalPrimitiveSize;
                        fAlign = 4;
                        nestedInlineStruct = true;
                    }
                    else if (nestedType is CLRType ct && ct.IsValueType)
                    {
                        if (ct.StructStorage == StructStorage.Inline)
                        {
                            nestedClrType = ct;
                            fSize = ct.TotalPrimitiveSize;
                            fAlign = ct.MaxAlignment;
                            nestedInlineStruct = true;
                        }
                        else
                        {
                            fSize = IntPtr.Size;
                            fAlign = IntPtr.Size;
                        }
                    }
                    else
                    {
                        // Reference type field or Boxed nested struct
                        fSize = IntPtr.Size;
                        fAlign = IntPtr.Size;
                    }
                }

                if (fAlign > maxAlignment) maxAlignment = fAlign;
                primitiveOffset = MemoryLayoutHelpers.AlignUp(primitiveOffset, fAlign);
                int fieldHash = fi.GetHashCode();
                neoFieldOffsets[fieldHash] = new ILTypeFieldOffset
                {
                    PrimitiveOffset = primitiveOffset,
                    ReferenceOffset = referenceOffset,
                };
                primitiveOffset += fSize;

                if (ft.IsPrimitive || ft.IsEnum || ft == typeof(IntPtr) || ft == typeof(UIntPtr))
                {
                    // no ref slots
                }
                else if (nestedInlineStruct)
                {
                    int nestedRefs = nestedIlType != null ? nestedIlType.TotalReferenceCount : nestedClrType.TotalReferenceCount;
                    // Record each reference slot's FieldInfo -- for nested Inline struct we don't have a direct
                    // FieldInfo per-slot; box/unbox for outer struct with nested-inline+ref fields is not exercised
                    // by any current test. Leave the entries as the containing field; Task 4 / 5 uses this only
                    // for top-level ref fields.
                    for (int r = 0; r < nestedRefs; r++)
                        refFieldInfos.Add(fi);
                    referenceOffset += nestedRefs;
                }
                else
                {
                    // Reference field (or Boxed nested struct treated as reference).
                    refFieldInfos.Add(fi);
                    referenceOffset++;
                }
            }

            totalPrimitiveSize = MemoryLayoutHelpers.AlignUp(primitiveOffset, maxAlignment);
            if (totalPrimitiveSize < 1) totalPrimitiveSize = 1;
            totalReferenceCount = referenceOffset;
            inlineRefFieldInfos = refFieldInfos.ToArray();
            this.MaxAlignment = maxAlignment;
        }

        /// <summary>
        /// True when the CLR type declares at least one instance method (including property getter/setter and
        /// override methods) beyond constructors and operator overloads. Used to decide Inline vs Boxed
        /// classification: a type with mutating instance methods needs Boxed storage so Unsafe.Unbox&lt;T&gt;
        /// keeps method calls zero-alloc; a pure-data struct is safe to Inline.
        /// </summary>
        bool HasInstanceMethods()
        {
            var methods = clrType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (m.IsConstructor) continue;
                if (m.IsStatic) continue;
                // Operator overloads are always static in C#; instance-side SpecialName methods here are
                // property accessors (get_/set_), which we DO count as instance methods.
                return true;
            }
            return false;
        }

        bool HasPrivateFields()
        {
            var fields = clrType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            return fields.Length > 0;
        }
#endif

        public int GetFieldIndex(string name)
        {
            if (fieldMapping == null)
                InitializeFields();
            if (fieldMapping.TryGetValue(name, out var index)) 
                return index;
            return 0;
        }
        public int GetFieldIndex(object token)
        {
            if (fieldMapping == null)
                InitializeFields();
            int idx;
            int hashCode = token.GetHashCode();
            if (fieldTokenMapping == null)
                fieldTokenMapping = new Dictionary<int, int>();
            if (fieldTokenMapping.TryGetValue(hashCode, out idx))
                return idx;
            FieldReference f = token as FieldReference;
            if (fieldMapping.TryGetValue(f.Name, out idx))
            {
                fieldTokenMapping[hashCode] = idx;
                return idx;
            }

            return -1;
        }

        public IType FindGenericArgument ( string key )
        {
            var o = this.Generic ( key );
            if ( o == null )
            {
                var aGenericParameters = this.TypeForCLR.GetGenericArguments ();
                if ( aGenericParameters !=null )
                {
                    for ( int i = 0; i < aGenericParameters.Length; i++ )
                    {
                        if ( aGenericParameters [ i ].Name == key )
                        {
                            return this.Generic ( "!" + i );
                        }
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
                    if ( this.genericArguments [ i ].Key == key )
                    {
                        return this.genericArguments [ i ].Value;
                    }
                }
            }

            return null;
        }
        public IMethod GetMethod(string name, int paramCount, bool declaredOnly = false)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    if (i.ParameterCount == paramCount)
                        return i;
                }
            }
            return null;
        }

        bool MatchGenericParameters(Type[] args, Type type, Type q, IType[] genericArguments)
        {
            if (type.IsGenericParameter)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == type)
                    {
                        return q == genericArguments[i].TypeForCLR;
                    }
                }
                throw new NotSupportedException();
            }
            else
            {
                if (type.IsArray)
                {
                    if (q.IsArray)
                    {
                        return MatchGenericParameters(args, type.GetElementType(), q.GetElementType(), genericArguments);
                    }
                    else
                        return false;
                }
                else if (type.IsByRef)
                {
                    if (q.IsByRef)
                    {
                        return MatchGenericParameters(args, type.GetElementType(), q.GetElementType(), genericArguments);
                    }
                    else
                        return false;
                }
                else if (type.IsGenericType)
                {
                    if (q.IsGenericType)
                    {
                        var t1 = type.GetGenericTypeDefinition();
                        var t2 = q.GetGenericTypeDefinition();
                        if (t1 == t2)
                        {
                            var argA = type.GetGenericArguments();
                            var argB = q.GetGenericArguments();
                            if (argA.Length == argB.Length)
                            {
                                for (int i = 0; i < argA.Length; i++)
                                {
                                    if (!MatchGenericParameters(args, argA[i], argB[i], genericArguments))
                                        return false;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return type == q;
            }
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null, bool declaredOnly = false)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            IMethod genericMethod = null;
            if (methods.TryGetValue(name, out lst))
            {
                var paramCount = param.Count;

                foreach (var i in lst)
                {
                    if (i.ParameterCount == paramCount)
                    {
                        bool match = true;
                        if (genericArguments != null && i.GenericParameterCount == genericArguments.Length)
                        {
                            for (int j = 0; j < paramCount; j++)
                            {
                                var p = i.Parameters[j].TypeForCLR;
                                var q = param[j].TypeForCLR;

                                if (i.Parameters[j].HasGenericParameter)
                                {
                                    //TODO should match the generic parameters;
                                    if (!MatchGenericParameters(i.GenericArgumentsCLR, i.ParametersCLR[j].ParameterType, q, genericArguments))
                                    {
                                        match = false;
                                        break;
                                    }
                                    else
                                        continue;
                                }
                                if (q != p)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match && genericMethod == null)
                            {
                                genericMethod = i;
                            }
                        }
                        else
                        {
                            var iGenericArguments = i.GenericArguments;
                            if (genericArguments == null)
                                match = iGenericArguments == null;
                            else
                            {
                                if (iGenericArguments == null)
                                    match = false;
                                else
                                    match = iGenericArguments.Length == genericArguments.Length;
                            }
                            if (!match)
                                continue;
                            for (int j = 0; j < paramCount; j++)
                            {
                                var typeA = /*param[j].TypeForCLR.IsByRef ? param[j].TypeForCLR.GetElementType() : */param[j]?.TypeForCLR;
                                var typeB = /*i.Parameters[j].TypeForCLR.IsByRef ? i.Parameters[j].TypeForCLR.GetElementType() : */i.Parameters[j].TypeForCLR;

                                if (typeA != null && typeA != typeB)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                try
                                {
                                    match = returnType == null || (i.ReturnType != null && i.ReturnType.TypeForCLR == returnType.TypeForCLR);
                                }
                                catch
                                {

                                }
                            }
                            if (match)
                            {

                                if (i.IsGenericInstance)
                                {
                                    if (iGenericArguments.Length == genericArguments.Length)
                                    {
                                        for (int j = 0; j < genericArguments.Length; j++)
                                        {
                                            if (iGenericArguments[j] != genericArguments[j])
                                            {
                                                match = false;
                                                break;
                                            }
                                        }
                                        if (match)
                                            return i;
                                    }
                                }
                                else
                                    return i;
                            }
                        }
                    }
                }
            }
            if (genericArguments != null && genericMethod != null)
            {
                var m = genericMethod.MakeGenericMethod(genericArguments);
                lst.Add((CLRMethod)m);
                return m;
            }
            return null;
        }
        public bool CanAssignTo(IType type)
        {
            if (this == type)
            {
                return true;
            }
            else
            {
                if (type is ILType)
                    return false;
                Type cT = type != null ? type.TypeForCLR : typeof(object);
                return cT.IsAssignableFrom(TypeForCLR);
            }
        }

        public IMethod GetConstructor(List<IType> param)
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
                        if (param[j].TypeForCLR != i.Parameters[j].TypeForCLR)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return i;
                    }
                }
            }

            return null;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            lock (this)
            {
                if (genericInstances == null)
                    genericInstances = new List<CLRType>();
                foreach (var i in genericInstances)
                {
                    bool match = true;
                    for (int j = 0; j < genericArguments.Length; j++)
                    {
                        if (i.genericArguments[j].Value != genericArguments[j].Value)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return i;
                }
                Type[] args = new Type[genericArguments.Length];
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    args[i] = genericArguments[i].Value.TypeForCLR;
                }

                Type newType = null;
#if UNITY_EDITOR || (DEBUG && !DISABLE_ILRUNTIME_DEBUG)
                try
                {
#endif
                    newType = clrType.MakeGenericType(args);
#if UNITY_EDITOR || (DEBUG && !DISABLE_ILRUNTIME_DEBUG)
                }
                catch (Exception e)
                {
                    string argString = "";
                    for (int i = 0; i < args.Length; i++)
                    {
                        argString += args[i].FullName + ", ";
                    }

                    argString = argString.Substring(0, argString.Length - 2);
                    throw new Exception(string.Format("MakeGenericType failed : {0}<{1}>", clrType.FullName, argString));
                }
#endif
                var res = new CLRType(newType, appdomain);
                res.genericArguments = genericArguments;

                genericInstances.Add(res);
                return res;
            }
        }

        public object CreateDefaultInstance()
        {
            if (createDefaultInstanceDelegate == null)
            {
                if (!AppDomain.CreateDefaultInstanceMap.TryGetValue(clrType, out createDefaultInstanceDelegate))
                {
                    createDefaultInstanceDelegate = () => Activator.CreateInstance(TypeForCLR);
                }
            }

            return createDefaultInstanceDelegate();
        }

        public object CreateArrayInstance(int size)
        {
            if (createArrayInstanceDelegate == null)
            {
                if (!AppDomain.CreateArrayInstanceMap.TryGetValue(clrType, out createArrayInstanceDelegate))
                {
                    createArrayInstanceDelegate = s => Array.CreateInstance(TypeForCLR, s);
                }
            }

            return createArrayInstanceDelegate(size);
        }

        public IType MakeByRefType()
        {
            if (byRefType == null)
            {
                Type t = clrType.MakeByRefType();
                byRefType = new CLRType(t, appdomain);
                ((CLRType)byRefType).elementType = this;
            }
            return byRefType;
        }
        public IType MakeArrayType(int rank)
        {
            if (arrayTypes == null)
            {
                arrayTypes = new Dictionary<int, IType>();
            }
            IType atype;
            if (!arrayTypes.TryGetValue(rank, out atype))
            {
                Type t = rank > 1 ? clrType.MakeArrayType(rank) : clrType.MakeArrayType();
                atype = new CLRType(t, appdomain);
                ((CLRType)atype).elementType = this;
                ((CLRType)atype).IsArray = true;
                ((CLRType)atype).ArrayRank = rank;
                arrayTypes[rank] = atype;
            }
            return atype;
        }

        public IType ResolveGenericType(IType contextType)
        {
            throw new NotImplementedException();
        }
        public ValueTypeInitInfo ValueTypeInitializationInfo
        {
            get
            {
                if (vtInitInfo == null)
                {
                    if (IsValueType)
                        vtInitInfo = new ValueTypeInitInfo(this);
                }
                return vtInitInfo;
            }
        }
        public void GetValueTypeSize(out int fieldCout, out int managedCount)
        {
            if (!valuetypeSizeCalculated)
            {
                var cnt = TotalFieldCount;
                valuetypeFieldCount = cnt + 1;
                valuetypeManagedCount = 0;
                for (int i = 0; i < cnt; i++)
                {
                    var it = OrderedFieldTypes[i] as CLRType;
                    if (it.IsValueType)
                    {
                        if (!it.IsPrimitive && !it.IsEnum)
                        {
                            if (it.ValueTypeBinder != null)
                            {
                                int fSize, fmCnt;
                                it.GetValueTypeSize(out fSize, out fmCnt);
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

                valuetypeSizeCalculated = true;
            }
            fieldCout = valuetypeFieldCount;
            managedCount = valuetypeManagedCount;
        }

        public override int GetHashCode()
        {
            if (hashCode == -1)
                hashCode = System.Threading.Interlocked.Add(ref instance_id, 1);
            return hashCode;
        }

        public override string ToString()
        {
            return clrType.ToString();
        }
    }
}
