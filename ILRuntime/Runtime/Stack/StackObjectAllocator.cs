using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Stack
{
    unsafe struct MemoryBlockInfo
    {
        public StackObject* RequestAddress;
        public StackObject* StartAddress;
        public int Size;
        public int ManagedIndex;
        public int ManagedCount;
    }

    public unsafe struct StackObjectAllocation
    {
        public StackObject* Address;
        public int ManagedIndex;
    }
    public unsafe delegate void StackObjectAllocateCallback(int size, out StackObject* ptr, out int managedIdx);
    public unsafe class StackObjectAllocator
    {
        MemoryBlockInfo[] freeBlocks;
        StackObjectAllocateCallback allocCallback;

        public StackObjectAllocator(StackObjectAllocateCallback cb)
        {
            allocCallback = cb;
        }

        void ExpandFreeList()
        {
            int expandSize = Math.Min(freeBlocks.Length, 32);
            MemoryBlockInfo[] newArr = new MemoryBlockInfo[freeBlocks.Length + expandSize];
            freeBlocks.CopyTo(newArr, 0);
            freeBlocks = newArr;
        }


        void InsertFreeMemoryBlock(ref MemoryBlockInfo block, int index)
        {
            if (index >= freeBlocks.Length - 1)
            {
                ExpandFreeList();
                freeBlocks[index] = block;
            }
            else
            {
                int freeSize = 0, freeMCount = 0, freeBlock = 0;
                for (int i = index; i < freeBlocks.Length; i++)
                {
                    if (freeBlocks[i].RequestAddress != null || freeBlocks[i].StartAddress == null)
                        break;
                    freeSize += freeBlocks[i].Size;
                    freeMCount += freeBlocks[i].ManagedCount;
                    freeBlock++;
                }
                if (freeBlock > 0)
                {
                    freeBlocks[index].Size = freeSize + block.Size;
                    freeBlocks[index].ManagedCount = freeMCount + block.ManagedCount;

                    int tail = index + freeBlock + 1;
                    var cnt = freeBlocks.Length;
                    if (tail < freeBlocks.Length)
                    {
                        Array.Copy(freeBlocks, tail, freeBlocks, index + 1, cnt - tail);
                    }
                    for (int i = cnt - freeBlock; i < cnt; i++)
                    {
                        freeBlocks[i] = default(MemoryBlockInfo);
                    }
                }
                else
                {
                    Array.Copy(freeBlocks, index, freeBlocks, index + 1, freeBlocks.Length - index - 1);
                    freeBlocks[index] = block;
                }
            }
        }

        void CheckBlockSizeAndAlloc(StackObject* ptr, ref MemoryBlockInfo block, int idx, int size, int managedSize, out StackObjectAllocation alloc)
        {
            if (block.Size > size || block.ManagedCount > managedSize)
            {
                MemoryBlockInfo newBlock = new MemoryBlockInfo()
                {
                    StartAddress = ILIntepreter.Minus(block.StartAddress, size),
                    Size = block.Size - size,
                    ManagedIndex = block.ManagedIndex + managedSize,
                    ManagedCount = block.ManagedCount - managedSize
                };
                InsertFreeMemoryBlock(ref newBlock, idx + 1);
                block.Size = size;
                block.ManagedCount = managedSize;
            }
            block.RequestAddress = ptr;
            alloc.Address = block.StartAddress;
            alloc.ManagedIndex = block.ManagedIndex;
        }

        void FreeBlock(int idx)
        {
            freeBlocks[idx].RequestAddress = null;
            var cnt = freeBlocks.Length;
            int freeSize = 0;
            int freeManaged = 0;
            int freeBlock = 0;
            for (int i = idx-1; i >= 0; i--)
            {
                if (freeBlocks[i].RequestAddress == null)
                {
                    idx = i;
                }
                else
                    break;
            }
            for (int i = idx + 1; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == null)
                {
                    freeSize += freeBlocks[i].Size;
                    freeManaged += freeBlocks[i].ManagedCount;
                    freeBlock++;
                }
                else
                    break;
            }
            if (freeBlock > 0)
            {
                freeBlocks[idx].Size += freeSize;
                freeBlocks[idx].ManagedCount += freeManaged;
                int tail = idx + freeBlock + 1;
                if (tail < freeBlocks.Length)
                {
                    Array.Copy(freeBlocks, tail, freeBlocks, idx + 1, cnt - tail);
                }
                for (int i = cnt - freeBlock; i < cnt; i++)
                {
                    freeBlocks[i] = default(MemoryBlockInfo);
                }
            }
        }

        public void Free(StackObject* ptr)
        {
            var cnt = freeBlocks.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == ptr)
                {
                    FreeBlock(i);
                    break;
                }
            }
        }

        public StackObjectAllocation Alloc(StackObject* ptr, int size, int managedSize)
        {
            if (freeBlocks == null)
                freeBlocks = new MemoryBlockInfo[8];
            int found = -1;
            int emptyIndex = -1;
            for (int i = 0; i < freeBlocks.Length; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                {
                    emptyIndex = i;
                    break;
                }
                if (freeBlocks[i].RequestAddress == ptr)
                {
                    FreeBlock(i);
                }
            }
            for (int i = 0; i < freeBlocks.Length; i++)
            {
                if (freeBlocks[i].StartAddress == null)
                    break;
                if (freeBlocks[i].RequestAddress == null)
                {
                    if (freeBlocks[i].Size >= size && freeBlocks[i].ManagedCount >= managedSize)
                    {
                        found = i;
                        break;
                    }
                }
            }
            StackObjectAllocation alloc;
            if (found >= 0)
            {
                CheckBlockSizeAndAlloc(ptr, ref freeBlocks[found], found, size, managedSize, out alloc);
            }
            else
            {
                if (emptyIndex == -1)
                {
                    emptyIndex = freeBlocks.Length;
                    ExpandFreeList();
                }
                allocCallback(size, out alloc.Address, out alloc.ManagedIndex);
                freeBlocks[emptyIndex] = new MemoryBlockInfo()
                {
                    StartAddress = alloc.Address,
                    RequestAddress = ptr,
                    Size = size,
                    ManagedCount = managedSize,
                    ManagedIndex = alloc.ManagedIndex
                };
            }
            return alloc;
        }
    }
}
