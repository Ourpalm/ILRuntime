using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Stack
{
    unsafe class RuntimeStack : IDisposable
    {
        ILIntepreter intepreter;
        StackObject* pointer;
        StackObject* endOfMemory;
        IntPtr nativePointer;
        List<object> managedStack = new List<object>();
        Stack<StackFrame> framse = new Stack<StackFrame>();
        const int MAXIMAL_STACK_OBJECTS = 1024 * 128;
        public RuntimeStack(ILIntepreter intepreter)
        {
            this.intepreter = intepreter;

            nativePointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(StackObject) * MAXIMAL_STACK_OBJECTS);
            pointer = (StackObject*)nativePointer.ToPointer();
            endOfMemory = pointer + MAXIMAL_STACK_OBJECTS;
        }

        ~RuntimeStack()
        {
            Dispose();
        }

        public StackObject* StackBase
        {
            get
            {
                return pointer;
            }
        }

        public List<object> ManagedStack { get { return managedStack; } }

        public StackFrame PushFrame(ILMethod method, StackObject* esp)
        {
            if (esp < pointer || esp >= endOfMemory)
                throw new StackOverflowException();
            if (framse.Count > 0 && framse.Peek().BasePointer > esp)
                throw new StackOverflowException();
            StackFrame res = new StackFrame();
            res.LocalVarPointer = esp;
            res.Method = method;
            res.BasePointer = esp + method.LocalVariableCount;
            framse.Push(res);
            return res;
        }

        public void PopFrame(ref StackFrame frame)
        {
            if (framse.Count > 0 && framse.Peek().BasePointer == frame.BasePointer)
                framse.Pop();
            else
                throw new NotSupportedException();
        }

        
        public void Dispose()
        {
            if (nativePointer != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(nativePointer);
                nativePointer = IntPtr.Zero;
            }
        }
    }
}
