using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.CLR.TypeSystem
{
    public unsafe class ValueTypeInitInfo
    {
        StackObject[] stackObjects;
        public int FieldCount { get; private set; }
        public int ManagedCount {  get; private set; }

        public int SizeInBytes {  get; private set; }

        int[] managedIndices;
        CLRType[] clrValueTypes;
        int[] valueTypeIndices;

        public ValueTypeInitInfo(IType type)
        {
            if (!type.IsValueType)
                throw new NotSupportedException();
            type.GetValueTypeSize(out var fieldCnt, out var mCnt);
            FieldCount = fieldCnt;
            SizeInBytes = fieldCnt * sizeof(StackObject);
            ManagedCount = mCnt;
            stackObjects = new StackObject[fieldCnt];
            int managedIdx = 0;
            List<int> valueTypes = null;
            fixed (StackObject* ptr = stackObjects)
            {
                var dst = ptr + fieldCnt - 1;
                AutoList mStack = new AutoList();
                RuntimeStack.InitializeValueTypeObject(type, dst, ref managedIdx, false, mStack);
                
                managedIdx = 0;
                for (int i = 0; i < FieldCount; i++)
                {
                    StackObject* so = dst - i;
                    int offset = FieldCount - 1 - i;
                    if (so->ObjectType == ObjectTypes.Object)
                    {
                        if (managedIndices == null)
                        {
                            managedIndices = new int[mCnt];
                            clrValueTypes = new CLRType[mCnt];
                        }
                        if (mStack[so->Value] != null)
                        {
                            clrValueTypes[managedIdx] = type.AppDomain.GetType(mStack[so->Value].GetType()) as CLRType;
                        }
                        managedIndices[managedIdx++] = offset;
                    }
                    else if (so->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        StackObject* val = *(StackObject**)&so->Value;
                        *(long*)&so->Value = val - ptr;
                        if (valueTypes == null)
                            valueTypes = new List<int>();
                        valueTypes.Add(offset);
                    }
                }
                if (valueTypes != null)
                    valueTypeIndices = valueTypes.ToArray();
            }
        }

        public void InitializeStackValueType(StackObject* ptr, int managedIdx, AutoList mStack)
        {
            var dst = ptr - (FieldCount - 1);
            fixed (StackObject* src = stackObjects)
            {
                Buffer.MemoryCopy(src, dst, SizeInBytes, SizeInBytes);
                if (managedIndices != null)
                {
                    for (int i = 0; i < managedIndices.Length; i++)
                    {
                        int idx = managedIndices[i];
                        object defaultValue = clrValueTypes[i] != null ? clrValueTypes[i].CreateDefaultInstance() : null;
                        int finalMIdx = managedIdx + dst[idx].Value;
                        dst[idx].Value = finalMIdx;
                        if (finalMIdx < mStack.Count)
                        {
                            mStack[finalMIdx] = defaultValue;
                        }
                        else
                        {
                            if (mStack.Count == finalMIdx)
                                mStack.Add(defaultValue);
                            else
                                throw new NotSupportedException();
                        }
                    }
                }
                if(valueTypeIndices != null)
                {
                    for(int i =0;i< valueTypeIndices.Length;i++)
                    {
                        int idx = valueTypeIndices[i];
                        *(StackObject**)&dst[idx].Value = dst + dst[idx].Value;
                    }
                }
            }
        }
    }
}
