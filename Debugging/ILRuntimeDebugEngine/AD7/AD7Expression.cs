using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Expression : IDebugExpression2
    {
        ILProperty property, parent;
        AD7Engine engine;
        string[] names;
        int curIdx;
        public AD7Expression(ILProperty property)
        {
            this.property = property;
        }

        public AD7Expression(AD7Engine engine, ILProperty root, string[] names)
        {
            this.engine = engine;
            this.parent = root;
            this.names = names;
            curIdx = 1;
        }

        public int Abort()
        {
            return Constants.E_NOTIMPL;
        }

        public int EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            return Constants.E_NOTIMPL;
        }

        public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback,
            out IDebugProperty2 ppResult)
        {
            if (property != null)
            {
                ppResult = property;
                return Constants.S_OK;
            }
            else
            {
                DoResolve();
                ppResult = null;
                return Constants.S_OK;
            }
        }
        public void DoResolve()
        {
            string member = names[curIdx];
            if(!parent.Children.TryGetValue(member, out parent))
            {
                VariableReference reference = parent.GetVariableReference();
                var info = engine.DebuggedProcess.ResolveVariable(reference, member);
                parent.Children[member] = new AD7.ILProperty(info);
            }
            curIdx++;
            if (curIdx < names.Length)
                DoResolve();
            else
                property = parent;
        }
    }
}
