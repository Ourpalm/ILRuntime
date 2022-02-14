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
                property = Resolve(exp, dwTimeout);
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

        ILProperty Resolve(EvalExpression exp, uint dwTimeout)
        {
            if(exp != null)
            {
                if(exp is NameExpression)
                {
                    return ResolveNameExpression((NameExpression)exp, dwTimeout);
                }
                if(exp is MemberAcessExpression)
                {
                    return ResolveMemberAccessExpression((MemberAcessExpression)exp, dwTimeout);
                }
                if(exp is IndexAccessExpression)
                {
                    return ResolveIndexAccessExpression((IndexAccessExpression)exp, dwTimeout);
                }
            }
            return null;
        }

        ILProperty ResolveNameExpression(NameExpression exp, uint dwTimeout)
        {
            ILProperty res = null;
            if (frame.Properties.TryGetValue(exp.Content, out res))
                return res;
            uint threadHash;
            frame.Thread.GetThreadId(out threadHash);

            if (exp.IsRoot)
            {
                var info = frame.Engine.DebuggedProcess.ResolveVariable(null, exp.Content, (int)threadHash, dwTimeout);
                if (info == null)
                {
                    info = new VariableInfo();
                    info.Name = exp.Content;
                    info.Value = "null";
                    info.TypeName = "null";
                }
                res = new AD7.ILProperty(frame.Engine, frame.Thread, info);
            }
            else
            {
                var info = VariableInfo.FromObject(null);
                info.Type = VariableTypes.FieldReference;
                info.Name = exp.Content;
                res = new ILProperty(frame.Engine, frame.Thread, info);
            }
            return res;
        }

        ILProperty ResolveMemberAccessExpression(MemberAcessExpression exp, uint dwTimeout)
        {
            ILProperty body = Resolve(exp.Body, dwTimeout);
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
                            if (exp.IsRoot)
                            {
                                var info = frame.Engine.DebuggedProcess.ResolveVariable(reference, member, (int)threadHash, dwTimeout);
                                prop = new AD7.ILProperty(frame.Engine, frame.Thread, info);
                                prop.Parent = body;
                                body.Children[member] = prop;
                            }
                            else
                            {
                                var info = VariableInfo.FromObject(null);
                                info.Type = VariableTypes.FieldReference;                                
                                info.Name = member;
                                prop = new ILProperty(frame.Engine, frame.Thread, info);
                                prop.Parent = body;
                            }
                        }
                        else if(reference.Type == VariableTypes.NotFound)
                        {
                            if (exp.IsRoot)
                            {
                                var info = frame.Engine.DebuggedProcess.ResolveVariable(null, reference.Name + "." + member, (int)threadHash, dwTimeout);
                                prop = new AD7.ILProperty(frame.Engine, frame.Thread, info);
                            }
                            else
                            {
                                var info = VariableInfo.FromObject(null);
                                info.Type = VariableTypes.FieldReference;
                                info.Name = reference.Name + "." + member;
                                prop = new ILProperty(frame.Engine, frame.Thread, info);
                            }
                        }

                    }
                }
            }
            else
                prop = new ILProperty(frame.Engine, frame.Thread, VariableInfo.NullReferenceExeption); 
            return prop;
        }

        ILProperty ResolveIndexAccessExpression(IndexAccessExpression exp, uint dwTimeout)
        {
            ILProperty body = Resolve(exp.Body, dwTimeout);
            ILProperty prop = null;
            if (body != null)
            {
                VariableReference reference = body.GetVariableReference();
                if (reference != null)
                {
                    uint threadHash;
                    frame.Thread.GetThreadId(out threadHash);

                    if (reference.Type < VariableTypes.Error)
                    {
                        var idxExp = exp.Index;
                        VariableReference idx = null;
                        if(idxExp is NameExpression)
                        {
                            int idxInt;
                            var content = ((NameExpression)idxExp).Content;
                            if(content == "true")
                            {
                                idx = VariableReference.True;
                            }
                            else if(content == "false")
                            {
                                idx = VariableReference.False;
                            }
                            else if(content == "null")
                            {
                                idx = VariableReference.Null;
                            }
                            else if (int.TryParse(content, out idxInt))
                            {
                                idx = VariableReference.GetInteger(idxInt);
                            }
                            else
                            {
                                var info = ResolveNameExpression((NameExpression)idxExp, dwTimeout);                                
                                idx = info.GetVariableReference();
                            }
                        }
                        else if(idxExp is StringLiteralExpression)
                        {
                            idx = VariableReference.GetString(((StringLiteralExpression)idxExp).Content);
                        }
                        else
                        {
                            var info = Resolve(idxExp, dwTimeout);
                            idx = info.GetVariableReference();
                        }
                        if (idx != null && idx.Type < VariableTypes.Error)
                        {
                            if (exp.IsRoot)
                            {
                                var info = frame.Engine.DebuggedProcess.ResolveIndexAccess(reference, idx, (int)threadHash, dwTimeout);
                                prop = new AD7.ILProperty(frame.Engine, frame.Thread, info);
                                prop.Parent = body;
                                prop.Parameters = new VariableReference[] { idx };
                            }
                            else
                            {
                                var info = VariableInfo.FromObject(null);
                                info.Type = VariableTypes.IndexAccess;
                                prop = new ILProperty(frame.Engine, frame.Thread, info);
                                prop.Parent = body;
                                prop.Parameters = new VariableReference[] { idx };
                            }
                        }
                    }
                }
            }
            else
                prop = new ILProperty(frame.Engine, frame.Thread, VariableInfo.NullReferenceExeption);
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
