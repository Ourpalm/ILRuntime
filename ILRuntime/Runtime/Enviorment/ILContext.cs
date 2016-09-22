using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
namespace ILRuntime.Runtime.Enviorment
{
    public unsafe struct ILContext
    {
        public AppDomain AppDomain { get; private set; }
        public StackObject* ESP { get; private set; }
        public List<object> ManagedStack { get; private set; }
        public IMethod Method { get; private set; }

        public ILContext(AppDomain domain, StackObject* esp, List<object> mStack, IMethod method)
        {
            AppDomain = domain;
            ESP = esp;
            ManagedStack = mStack;
            Method = method;
        }
    }
}
