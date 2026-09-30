#if ENABLE_NEO_MODE
using System;
using System.Runtime.CompilerServices;
using ILRuntime.CLR.TypeSystem;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILRuntime.Runtime.Intepreter
{
    public unsafe partial class ILIntepreter
    {
        // Allocated only for CLR field addresses needing parent-location writeback,
        // and CLR static field addresses. IL addresses use the ordinary Ref Slot.
        // Retaining the parent location rather than a detached box preserves aliases.
        sealed class CLRFieldAddressFallback
        {
            internal int ParentIndex, ParentOffset;
            internal object ParentOwner;
            internal IType ParentType;
            internal CLRType StaticType;
        }

        static int FieldHash(CLRType type, int offset)
        {
            if (type.StructStorage == StructStorage.Inline)
                foreach (var field in type.Fields)
                    if (type.GetFieldPrimitiveOffset(field.Key) == offset) return field.Key;
            return offset;
        }

        static int StorageSize(IType type)
        {
            if (type is ILType il && il.IsValueType) return il.TotalPrimitiveSize;
            if (type is CLRType clr && clr.IsValueType && clr.StructStorage == StructStorage.Inline)
                return clr.TotalPrimitiveSize;
            return MemoryLayoutHelpers.GetPrimitiveSizeFromClrType(type.TypeForCLR);
        }

        void CreateFieldAddress(byte* destination, byte* receiver, int kind, int fieldOffset,
            int anchor, IType receiverType, AutoList refs)
        {
            int index = kind == 3 ? -1 : *(int*)receiver;
            int offset = kind == 3 ? (int)(receiver - (byte*)stack.StackBase) :
                kind == 1 ? *(int*)(receiver + 4) : 0;
            if (kind == 0)
            {
                if (index < 0 || refs[index] == null) throw new NullReferenceException();
                refs[anchor] = refs[index];
                index = anchor;
            }
            else if (index >= 0 && refs[index] is ILTypeInstance[] ilArray)
            {
                CheckNeoArrayIndex(ilArray, offset);
                refs[anchor] = ilArray[offset];
                index = anchor;
                offset = 0;
            }
            else if (index >= 0 && !(refs[index] is ILTypeInstance) &&
                (refs[index] is Array || offset != 0 || refs[index] is CLRFieldAddressFallback))
            {
                refs[anchor] = CreateCLRFieldAddressFallback(index, offset, refs[index], receiverType);
                index = anchor;
                offset = 0;
            }
            *(int*)destination = index;
            *(int*)(destination + 4) = offset + fieldOffset;
        }

        void CommitIndirectOwner(AutoList refs, int index, object owner)
        {
            if (!(refs[index] is CLRFieldAddressFallback address))
            {
                refs[index] = owner;
                return;
            }
            if (address.StaticType == null)
                WriteAddress(address.ParentIndex, address.ParentOffset, address.ParentType, owner, refs, address.ParentOwner);
        }

        static CLRFieldAddressFallback CreateCLRFieldAddressFallback(int index, int offset, object owner, IType type)
        {
            return new CLRFieldAddressFallback { ParentIndex = index, ParentOffset = offset,
                ParentOwner = owner, ParentType = type };
        }

        static CLRFieldAddressFallback CreateCLRStaticFieldAddressFallback(CLRType type)
        {
            return new CLRFieldAddressFallback { StaticType = type };
        }

        object ReadCLRFieldAddressFallback(CLRFieldAddressFallback address, int offset, AutoList refs)
        {
            object parent = address.StaticType == null
                ? ReadAddress(address.ParentIndex, address.ParentOffset, address.ParentType, refs, address.ParentOwner) : null;
            var ownerType = address.StaticType ?? (CLRType)address.ParentType;
            return ownerType.GetFieldValue(address.StaticType != null ? offset : FieldHash(ownerType, offset), parent);
        }

        void CommitCLRFieldAddressFallback(CLRFieldAddressFallback address, int offset, object value, AutoList refs)
        {
            object parent = address.StaticType == null
                ? ReadAddress(address.ParentIndex, address.ParentOffset, address.ParentType, refs, address.ParentOwner) : null;
            var ownerType = address.StaticType ?? (CLRType)address.ParentType;
            ownerType.SetFieldValue(address.StaticType != null ? offset : FieldHash(ownerType, offset), ref parent, value);
            if (address.StaticType == null)
                WriteAddress(address.ParentIndex, address.ParentOffset, address.ParentType, parent, refs, address.ParentOwner);
        }

        int ReferenceOffset(IType type, int offset, bool isStatic = false)
        {
            if (offset == 0 && !isStatic) return 0;
            if (type is ILType il)
            {
                var fields = isStatic ? il.StaticFieldTypes : il.FieldTypes;
                for (int i = fields.Length - 1; i >= 0; i--)
                {
                    var field = isStatic ? il.GetStaticFieldOffset(i) : il.GetFieldOffset(i);
                    if (field.PrimitiveOffset <= offset)
                        return field.ReferenceOffset + ReferenceOffset(fields[i], offset - field.PrimitiveOffset);
                }
            }
            else if (type is CLRType clr)
            {
                foreach (var field in clr.Fields)
                {
                    int start = clr.GetFieldPrimitiveOffset(field.Key);
                    var ft = AppDomain.GetType(field.Value.FieldType);
                    if (start >= 0 && start <= offset && offset < start + StorageSize(ft))
                        return clr.GetFieldReferenceOffset(field.Key) + ReferenceOffset(ft, offset - start);
                }
            }
            throw new InvalidProgramException("Neo managed reference does not identify a field.");
        }

        int FrameReferenceIndex(int offset)
        {
            byte* target = (byte*)stack.StackBase + offset;
            foreach (var frame in stack.Frames)
            {
                byte* start = (byte*)frame.BasePointer;
                ref readonly var layout = ref frame.Method.CompiledFrame;
                if (target < start || target >= start + layout.TotalStructSize) continue;
                for (int i = 0; i < layout.LocalInfos.Length; i++)
                {
                    var slot = layout.LocalInfos[i];
                    int delta = (int)(target - start) - slot.Offset;
                    if (!slot.IsRef && delta >= 0 && delta < slot.Size)
                        return frame.ManagedStackBase + slot.RefOffset +
                            ReferenceOffset(layout.LocalTypes[i], delta);
                }
            }
            throw new InvalidProgramException("Neo managed pointer targets an inactive frame.");
        }

        internal object ReadValue(byte* source, IType type, AutoList refs, int referenceBase)
        {
            if (type.IsPrimitive || type.IsEnum)
            {
                long value = 0;
                Unsafe.CopyBlockUnaligned(&value, source, (uint)StorageSize(type));
                return ReadNeoPrimitive((byte*)&value, type is ILType enumType ? enumType.FieldTypes[0] : type);
            }
            if (type is ILType il && il.IsValueType)
            {
                var value = il.Instantiate(true);
                CopyFrameToIL(source, 0, 0, il.TotalPrimitiveSize, il.TotalReferenceCount, refs, referenceBase, value);
                return value;
            }
            if (type is CLRType clr && clr.IsValueType && clr.StructStorage == StructStorage.Inline)
            {
                object boxed = Activator.CreateInstance(clr.TypeForCLR);
                CopyFrameToBoxedClrObjectStatic(boxed, clr, source, refs, referenceBase);
                return boxed;
            }
            int index = *(int*)source;
            return index < 0 ? null : refs[index];
        }

        internal void WriteValue(byte* destination, IType type, object value, AutoList refs, int referenceBase)
        {
            if (type.IsPrimitive || type.IsEnum)
            {
                long bytes = 0;
                WriteNeoPrimitive((byte*)&bytes, type is ILType enumType ? enumType.FieldTypes[0] : type, value);
                Unsafe.CopyBlockUnaligned(destination, &bytes, (uint)StorageSize(type));
                return;
            }
            if (type is ILType il && il.IsValueType)
            {
                CopyILToFrame((ILTypeInstance)value, destination, 0, 0,
                    il.TotalPrimitiveSize, il.TotalReferenceCount, refs, referenceBase);
                return;
            }
            if (type is CLRType clr && clr.IsValueType && clr.StructStorage == StructStorage.Inline)
            {
                CopyBoxedClrObjectToFrameStatic(value, clr, destination, refs, referenceBase);
                return;
            }
            refs[referenceBase] = type.IsValueType ? RuntimeHelpers.GetObjectValue(value) : value;
            *(int*)destination = value == null ? -1 : referenceBase;
        }

        internal object ReadAddress(int index, int offset, IType type, AutoList refs, object anchoredOwner = null)
        {
            if (index == -1)
            {
                if (!type.IsValueType && !type.IsPrimitive)
                    return refs[FrameReferenceIndex(offset)];
                return ReadValue((byte*)stack.StackBase + offset, type, refs,
                    type.IsPrimitive || type.IsEnum ? 0 : FrameReferenceIndex(offset));
            }
            object owner = anchoredOwner ?? refs[index];
            if (owner is ILTypeInstance il)
            {
                if (!type.IsValueType && !type.IsPrimitive)
                    return il.ManagedObjects[ReferenceOffset(il.Type, offset, il is ILTypeStaticInstance)];
                fixed (byte* bytes = il.Primitives)
                    return ReadValue(bytes + offset, type, il.ManagedObjects,
                        ReferenceOffset(il.Type, offset, il is ILTypeStaticInstance));
            }
            if (owner is Array array) { CheckNeoArrayIndex(array, offset); return array.GetValue(offset); }
            if (owner is CLRFieldAddressFallback address)
                return ReadCLRFieldAddressFallback(address, offset, refs);
            if (owner == null) throw new NullReferenceException();
            return offset == 0 && type.IsValueType ? owner : ResolveClrOwnerType(owner).GetFieldValue(offset, owner);
        }

        internal void WriteAddress(int index, int offset, IType type, object value, AutoList refs, object anchoredOwner = null)
        {
            if (index == -1)
            {
                WriteValue((byte*)stack.StackBase + offset, type, value, refs,
                    type.IsPrimitive || type.IsEnum ? 0 : FrameReferenceIndex(offset));
                return;
            }
            object owner = anchoredOwner ?? refs[index];
            if (owner is ILTypeInstance il)
            {
                fixed (byte* bytes = il.Primitives)
                    WriteValue(bytes + offset, type, value, il.ManagedObjects,
                        ReferenceOffset(il.Type, offset, il is ILTypeStaticInstance));
            }
            else if (owner is Array array)
            {
                CheckNeoArrayIndex(array, offset);
                IType elementType = GetNeoArrayElementType(array);
                if (!elementType.IsValueType && elementType is ILType ilElement && value != null &&
                    (!(value is ILTypeInstance candidate) || !candidate.CanAssignTo(ilElement)))
                    throw new ArrayTypeMismatchException();
                object stored = type.IsValueType && value is ILTypeInstance ilValue ? ilValue.Clone() :
                    type.IsValueType ? RuntimeHelpers.GetObjectValue(value) : value;
                try { array.SetValue(stored, offset); }
                catch (InvalidCastException) { throw new ArrayTypeMismatchException(); }
            }
            else if (owner is CLRFieldAddressFallback address)
            {
                CommitCLRFieldAddressFallback(address, offset, value, refs);
            }
            else if (offset == 0 && type.IsValueType)
            {
                if (anchoredOwner == null) refs[index] = RuntimeHelpers.GetObjectValue(value);
                else
                    foreach (var field in ((CLRType)type).Fields.Values)
                        if (!field.IsStatic) field.SetValue(anchoredOwner, field.GetValue(value));
            }
            else
            {
                if (owner == null) throw new NullReferenceException();
                ResolveClrOwnerType(owner).SetFieldValue(offset, ref owner, value);
                if (anchoredOwner == null) refs[index] = owner;
            }
        }

        public T ReadRef<T>(byte* slot, AutoList refs)
        {
            return (T)ReadAddress(*(int*)slot, *(int*)(slot + 4), AppDomain.GetType(typeof(T)), refs);
        }

        public void WriteRef<T>(byte* slot, AutoList refs, T value)
        {
            WriteAddress(*(int*)slot, *(int*)(slot + 4), AppDomain.GetType(typeof(T)), value, refs);
        }

        // CLR may reorder managed fields even on sequential structs. Neo offsets
        // describe our frame, and must never be used to overwrite GC pointer bytes.
        static void CopyClrFields(object boxed, CLRType type, byte* bytes, AutoList refs, int refBase, bool toFrame)
        {
            foreach (var entry in type.Fields)
            {
                var field = entry.Value;
                if (field.IsStatic) continue;
                int offset = type.GetFieldPrimitiveOffset(entry.Key);
                int refOffset = Math.Max(0, type.GetFieldReferenceOffset(entry.Key));
                IType ft = type.AppDomain.GetType(field.FieldType);
                if (ft.IsPrimitive || ft.IsEnum)
                {
                    long temp = 0;
                    int size = StorageSize(ft);
                    if (toFrame)
                    {
                        WriteNeoPrimitive((byte*)&temp, ft, field.GetValue(boxed));
                        Unsafe.CopyBlockUnaligned(bytes + offset, &temp, (uint)size);
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(&temp, bytes + offset, (uint)size);
                        field.SetValue(boxed, ReadNeoPrimitive((byte*)&temp, ft));
                    }
                }
                else if (ft is CLRType nested && nested.IsValueType && nested.StructStorage == StructStorage.Inline)
                {
                    object value = toFrame ? field.GetValue(boxed) : Activator.CreateInstance(field.FieldType);
                    CopyClrFields(value, nested, bytes + offset, refs, refBase + refOffset, toFrame);
                    if (!toFrame) field.SetValue(boxed, value);
                }
                else if (toFrame)
                {
                    refs[refBase + refOffset] = field.GetValue(boxed);
                    Unsafe.InitBlockUnaligned(bytes + offset, 0, (uint)IntPtr.Size);
                }
                else field.SetValue(boxed, refs[refBase + refOffset]);
            }
        }
    }
}
#endif
