using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime
{
    public enum ILRuntimeJITFlags
    {
        None,
        /// <summary>
        /// Method will be JIT when method is called multiple time
        /// </summary>
        JITOnDemand = 1,
        /// <summary>
        /// Method will be JIT immediately when called, instead of progressively warm up
        /// </summary>
        JITImmediately = 2,
        /// <summary>
        /// Method will not be JIT when called
        /// </summary>
        NoJIT = 4,
        /// <summary>
        /// Method will always be inlined when called
        /// </summary>
        ForceInline = 8,
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ILRuntimeJITAttribute : Attribute
    {
        ILRuntimeJITFlags flags;

        public ILRuntimeJITFlags Flags { get { return flags; } }
        public ILRuntimeJITAttribute()
        {
            this.flags = ILRuntimeJITFlags.JITOnDemand;
        }

        public ILRuntimeJITAttribute(ILRuntimeJITFlags flags)
        {
            this.flags = flags;
        }
    }
}
