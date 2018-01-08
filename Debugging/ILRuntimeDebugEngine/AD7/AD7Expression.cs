using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;
using ILRuntimeDebugEngine.Expressions;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Expression : IDebugExpression2
    {
        ILProperty property;
        AD7StackFrame frame;
        EvalExpression exp;

        /*string[] names;
        bool resolved;
        int curIdx;*/
        public AD7Expression(ILProperty property)
        {
            this.property = property;
        }

        public AD7Expression(AD7StackFrame frame, EvalExpression exp)
        {
            this.frame = frame;
            this.exp = exp;
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
                property = Resolve(exp);
                ppResult = property;
                if (property != null)
                    return Constants.S_OK;
                else
                    return Constants.E_FAIL;
            }
        }

        ILProperty Resolve(EvalExpression exp)
        {
            if(exp != null)
            {
                if(exp is NameExpression)
                {
                    return ResolveNameExpression((NameExpression)exp);
                }
                if(exp is MemberAcessExpression)
                {
                    return ResolveMemberAccessExpression((MemberAcessExpression)exp);
                }
            }
            return null;
        }

        ILProperty ResolveNameExpression(NameExpression exp)
        {
            ILProperty res = null;
            if (frame.Properties.TryGetValue(exp.Content, out res))
                return res;
            uint threadHash;
            frame.Thread.GetThreadId(out threadHash);

            var info = frame.Engine.DebuggedProcess.ResolveVariable(null, exp.Content, (int)threadHash);
            if (info == null)
            {
                info = new VariableInfo();
                info.Name = exp.Content;
                info.Value = "null";
                info.TypeName = "null";
            }
            res = new AD7.ILProperty(info);

            return res;
        }

        ILProperty ResolveMemberAccessExpression(MemberAcessExpression exp)
        {
            ILProperty body = Resolve(exp.Body);
            string member = exp.Member;
            ILProperty prop;
            if (body != null)
            {
                if (!body.Children.TryGetValue(member, out prop))
                {
                    VariableReference reference = body.GetVariableReference();
                    if (reference != null)
                    {
                        uint threadHash;
                        frame.Thread.GetThreadId(out threadHash);

                        if (reference.Type < VariableTypes.Error)
                        {                            
                            var info = frame.Engine.DebuggedProcess.ResolveVariable(reference, member, (int)threadHash);
                            if (info == null)
                            {
                                info = new VariableInfo();
                                info.Name = member;
                                info.Value = "null";
                                info.TypeName = "null";
                            }
                            prop = new AD7.ILProperty(info);
                            prop.Parent = body;
                            body.Children[member] = prop;
                        }
                        else if(reference.Type == VariableTypes.NotFound)
                        {
                            var info = frame.Engine.DebuggedProcess.ResolveVariable(null, reference.Name + "." + member, (int)threadHash);
                            prop = new AD7.ILProperty(info);                            
                        }

                    }
                }
            }
            else
                prop = new ILProperty(VariableInfo.NullReferenceExeption); 
            return prop;
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
