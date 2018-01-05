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
        AD7Thread thread;
        string[] names;
        bool resolved;
        int curIdx;
        public AD7Expression(ILProperty property)
        {
            this.property = property;
        }

        public AD7Expression(AD7Engine engine, AD7Thread thread, ILProperty root, string[] names)
        {
            this.engine = engine;
            this.parent = root;
            this.thread = thread;
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
            if (property != null || resolved)
            {
                ppResult = property;
                return Constants.S_OK;
            }
            else
            {
                DoResolve();
                ppResult = property;
                return Constants.S_OK;
            }
        }
        public void DoResolve()
        {
            string member = names[curIdx];
            ILProperty prop;
            if (!parent.Children.TryGetValue(member, out prop))
            {
                VariableReference reference = parent.GetVariableReference();
                if (reference != null)
                {
                    if (reference.Type != VariableTypes.Error)
                    {
                        uint threadHash;
                        thread.GetThreadId(out threadHash);
                        var info = engine.DebuggedProcess.ResolveVariable(reference, member, (int)threadHash);
                        if (info == null)
                        {
                            info = new VariableInfo();
                            info.Name = member;
                            info.Value = "null";
                            info.TypeName = "null";
                        }
                        prop = new AD7.ILProperty(info);
                    }                    
                }
                else
                    prop = null;
                if (prop != null)
                    prop.Parent = parent;
                parent.Children[member] = prop;
            }
            parent = prop;
            curIdx++;
            if (curIdx < names.Length && parent != null)
                DoResolve();
            else
            {
                resolved = true;
                property = parent;
            }
        }
    }
}
