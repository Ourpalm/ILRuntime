using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Expressions;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Expression : IDebugExpression2
    {
        ILProperty property;
        AD7StackFrame frame;
        EvalExpression exp;
        string oriExp;

        /*string[] names;
        bool resolved;
        int curIdx;*/
        public AD7Expression(ILProperty property)
        {
            this.property = property;
        }

        public AD7Expression(AD7StackFrame frame, EvalExpression exp, string oriExp)
        {
            this.frame = frame;
            this.exp = exp;
            this.oriExp = oriExp;
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
                property = frame.Engine.DebuggedProcess.Resolve(frame, exp, dwTimeout) as ILProperty;
                ppResult = property;
                if (property != null)
                {
                    if (string.IsNullOrEmpty(property.Name))
                        property.Name = oriExp;
                    return Constants.S_OK;
                }
                else
                    return Constants.E_FAIL;
            }
        }
        /*public void DoResolve()
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
        }*/

    }
}