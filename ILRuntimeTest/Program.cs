using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ILRuntime.Runtime.Stack;
using System.Diagnostics;

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
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Console.BufferHeight = 3000;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestMainForm()); 
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
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
            lst.Add(allocator.Alloc((StackObject*)8, 8, 3));
            managedIdx += 10;

            lst.Add(allocator.Alloc((StackObject*)3, 8, 10));
            managedIdx += 1;
            lst.Add(allocator.Alloc((StackObject*)9, 8, 3));

            lst.Add(allocator.Alloc((StackObject*)2, 2, 3));
            managedIdx += 1;

            lst.Add(allocator.Alloc((StackObject*)4, 4, 4));
            managedIdx += 1;


            allocator.Free((StackObject*)2);
            allocator.Free((StackObject*)4);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Random rand = new Random();
            for (int i = 0; i < 1000000; i++)
            {
                //allocator.Alloc((StackObject*)2, 5, 2);
                /*switch (i % 3)
                {
                    case 0:
                        allocator.Alloc((StackObject*)2, 5, 2);
                        break;
                    case 1:
                        allocator.Alloc((StackObject*)3, 5, 2);
                        break;
                    case 2:
                        allocator.Alloc((StackObject*)2, 5, 2);
                        break;
                }*/
                allocator.Alloc((StackObject*)rand.Next(2, 5), rand.Next(2, 5), rand.Next(2, 5));
            }
            sw.Stop();
            Console.WriteLine("Elapsed:" + sw.ElapsedMilliseconds);
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                rand.Next(2, 8);
                rand.Next(2, 10);
                rand.Next(2, 10);
            }
            Console.WriteLine("Elapsed:" + sw.ElapsedMilliseconds);
            allocator.FreeBefore((StackObject*)0xfff40);

        }

        static void Alloc(int size, out StackObject* dst, out int mIdx)
        {
            dst = curPtr;
            curPtr -= size;
            mIdx = managedIdx;
        }
    }
}
