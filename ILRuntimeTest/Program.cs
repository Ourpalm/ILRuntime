using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ILRuntime.Runtime.Stack;

namespace ILRuntimeTest
{
    unsafe static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            TestAllocator();
            Console.BufferHeight = 3000;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestMainForm());
        }

        static StackObject* basePtr;
        static StackObject* curPtr;
        static int managedIdx;
        static void TestAllocator()
        {
            basePtr = (StackObject*)0x100000;
            curPtr = basePtr;
            managedIdx = 0;
            List<StackObjectAllocation> lst = new List<StackObjectAllocation>();
            StackObjectAllocator allocator = new StackObjectAllocator(Alloc);
            lst.Add(allocator.Alloc((StackObject*)2, 5, 10));
            managedIdx += 10;

            lst.Add(allocator.Alloc((StackObject*)3, 2, 1));
            managedIdx += 1;

            lst.Add(allocator.Alloc((StackObject*)2, 2, 1));
            managedIdx += 1;

            lst.Add(allocator.Alloc((StackObject*)4, 2, 1));
            managedIdx += 1;

            allocator.Free((StackObject*)2);
            allocator.Free((StackObject*)4);

        }

        static void Alloc(int size, out StackObject* dst, out int mIdx)
        {
            dst = curPtr;
            curPtr -= size;
            mIdx = managedIdx;            
        }
    }
}
