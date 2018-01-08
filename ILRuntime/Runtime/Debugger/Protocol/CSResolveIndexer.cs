using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSResolveIndexer
    {
        public int ThreadHashCode { get; set; }
        public string Index { get; set; }
        public VariableReference IndexRef { get; set; }
        public VariableReference Body { get; set; }
    }
}
