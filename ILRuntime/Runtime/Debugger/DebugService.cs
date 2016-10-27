using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        DebuggerServer server;
        Runtime.Enviorment.AppDomain domain;
        Dictionary<int, LinkedList<BreakpointInfo>> activeBreakpoints = new Dictionary<int, LinkedList<BreakpointInfo>>();
        Dictionary<int, BreakpointInfo> breakpointMapping = new Dictionary<int, BreakpointInfo>();
        AutoResetEvent evt = new AutoResetEvent(false);
        
        public Action<string> OnBreakPoint;

        public Enviorment.AppDomain AppDomain { get { return domain; } }

        public bool IsDebuggerAttached
        {
            get
            {
#if DEBUG
                return (server != null && server.IsAttached);
#else
                return false;
#endif
            }
        }

        public DebugService(Runtime.Enviorment.AppDomain domain)
        {
            this.domain = domain;
        }

        /// <summary>
        /// Start Debugger Server
        /// </summary>
        /// <param name="port">Port to listen on</param>
        public void StartDebugService(int port)
        {
#if DEBUG
            server = new Debugger.DebuggerServer(this);
            server.Port = port;
            server.Start();
#endif
        }

        /// <summary>
        /// 中断运行
        /// </summary>
        /// <param name="intpreter"></param>
        /// <param name="ex"></param>
        /// <returns>如果挂的有调试器则返回true</returns>
        internal bool Break(ILIntepreter intpreter, Exception ex = null)
        {
            BreakPointContext ctx = new BreakPointContext();
            ctx.Interpreter = intpreter;
            ctx.Exception = ex;

            curBreakpoint = ctx;

            if (OnBreakPoint != null)
            {
                OnBreakPoint(ctx.DumpContext());
                return true;
            }
            return false;
        }

        internal string GetStackTrance(ILIntepreter intepreper)
        {
            StringBuilder sb = new StringBuilder();
            ILRuntime.CLR.Method.ILMethod m;
            StackFrame[] frames = intepreper.Stack.Frames.ToArray();
            Mono.Cecil.Cil.Instruction ins = null;
            if (frames[0].Address != null)
            {
                ins = frames[0].Method.Definition.Body.Instructions[frames[0].Address.Value];
                sb.AppendLine(ins.ToString());
            }
            for (int i = 0; i < frames.Length; i++)
            {
                var f = frames[i];
                m = f.Method;
                string document = "";
                if (f.Address != null)
                {
                    ins = m.Definition.Body.Instructions[f.Address.Value];
                    var seq = FindSequencePoint(ins);
                    if (seq != null)
                    {
                        document = string.Format("{0}:Line {1}", seq.Document.Url, seq.StartLine);
                    }
                }
                sb.AppendFormat("at {0} {1}\r\n", m, document);
            }

            return sb.ToString();
        }

        internal unsafe string GetThisInfo(ILIntepreter intepreter)
        {
            var topFrame = intepreter.Stack.Frames.Peek();
            var arg = Minus(topFrame.LocalVarPointer, topFrame.Method.ParameterCount);
            if (topFrame.Method.HasThis)
                arg--;
            if (arg->ObjectType == ObjectTypes.StackObjectReference)
                arg = *(StackObject**)&arg->Value;
            ILTypeInstance instance = intepreter.Stack.ManagedStack[arg->Value] as ILTypeInstance;
            if (instance == null)
                return "null";
            var fields = instance.Type.TypeDefinition.Fields;
            int idx = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
            {
                var f = fields[i];
                if (f.IsStatic)
                    continue;
                var field = instance.Fields[idx];
                var v = StackObject.ToObject(&field, intepreter.AppDomain, instance.ManagedObjects);
                if (v == null)
                    v = "null";
                string name = f.Name;
                sb.AppendFormat("{0} {1} = {2}", f.FieldType.Name, name, v);
                if ((idx % 3 == 0 && idx != 0) || idx == instance.Fields.Length - 1)
                    sb.AppendLine();
                else
                    sb.Append(", ");
                idx++;
            }
            return sb.ToString();
        }

        internal unsafe string GetLocalVariableInfo(ILIntepreter intepreter)
        {
            StackFrame topFrame = intepreter.Stack.Frames.Peek();
            var m = topFrame.Method;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m.LocalVariableCount; i++)
            {
                var lv = m.Definition.Body.Variables[i];
                var val = Add(topFrame.LocalVarPointer, i);
                var v = StackObject.ToObject(val, intepreter.AppDomain, intepreter.Stack.ManagedStack);
                if (v == null)
                    v = "null";
                string name = string.IsNullOrEmpty(lv.Name) ? "v" + lv.Index : lv.Name;
                sb.AppendFormat("{0} {1} = {2}", lv.VariableType.Name, name, v);
                if ((i % 3 == 0 && i != 0) || i == m.LocalVariableCount - 1)
                    sb.AppendLine();
                else
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        internal static Mono.Cecil.Cil.SequencePoint FindSequencePoint(Mono.Cecil.Cil.Instruction ins)
        {
            Mono.Cecil.Cil.Instruction cur = ins;
            while (cur.SequencePoint == null && cur.Previous != null)
                cur = cur.Previous;

            return cur.SequencePoint;
        }

        unsafe StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }

        unsafe StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        internal void NotifyModuleLoaded(string moduleName)
        {
            if (server != null && server.IsAttached)
                server.NotifyModuleLoaded(moduleName);
        }

        internal void SetBreakPoint(int methodHash, int bpHash, int startLine)
        {
            lock (activeBreakpoints)
            {
                LinkedList<BreakpointInfo> lst;
                if(!activeBreakpoints.TryGetValue(methodHash, out lst))
                {
                    lst = new LinkedList<Debugger.BreakpointInfo>();
                    activeBreakpoints[methodHash] = lst;
                }

                BreakpointInfo bpInfo = new BreakpointInfo();
                bpInfo.BreakpointHashCode = bpHash;
                bpInfo.MethodHashCode = methodHash;
                bpInfo.StartLine = startLine;

                lst.AddLast(bpInfo);
                breakpointMapping[bpHash] = bpInfo;
            }
        }

        internal void CheckShouldBreak(ILMethod method, ILIntepreter intp, int ip)
        {
            if (server != null && server.IsAttached)
            {
                int methodHash = method.GetHashCode();
                lock (activeBreakpoints)
                {
                    LinkedList<BreakpointInfo> lst;
                    if (activeBreakpoints.TryGetValue(methodHash, out lst))
                    {
                        var sp = FindSequencePoint(method.Definition.Body.Instructions[ip]);
                        if (sp != null)
                        {
                            foreach (var i in lst)
                            {
                                if (i.StartLine == sp.StartLine)
                                {
                                    server.SendSCBreakpointHit(intp.GetHashCode(), i.BreakpointHashCode);
                                    //Breakpoint hit
                                    evt.WaitOne();
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void ThreadStarted(ILIntepreter intp)
        {
            if (server != null && server.IsAttached)
            {
                server.SendSCThreadStarted(intp.GetHashCode());
            }
        }

        internal void ThreadEnded(ILIntepreter intp)
        {
            if (server != null && server.IsAttached)
            {
                server.SendSCThreadEnded(intp.GetHashCode());
            }
        }
    }
}
