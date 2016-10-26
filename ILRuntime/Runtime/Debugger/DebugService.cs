using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        DebuggerServer server;
        static DebugService instance = new DebugService();

        public Action<string> OnBreakPoint;
        public static DebugService Instance { get { return instance; } }

        /// <summary>
        /// Start Debugger Server
        /// </summary>
        /// <param name="port">Port to listen on</param>
        public void StartDebugService(int port)
        {
#if DEBUG
            server = new Debugger.DebuggerServer();
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

        Mono.Cecil.Cil.SequencePoint FindSequencePoint(Mono.Cecil.Cil.Instruction ins)
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
    }
}
