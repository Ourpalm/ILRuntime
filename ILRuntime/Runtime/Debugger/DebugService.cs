using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        static DebugService instance = new DebugService();

        public Action<string> OnBreakPoint;
        public static DebugService Instance { get { return instance; } }

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
            for (int i = 0; i < frames.Length; i++)
            {
                var f = frames[i];
                m = f.Method;
                var ins = m.Definition.Body.Instructions[f.Address.Value];
                string document = "";
                var seq = FindSequencePoint(ins);
                if (seq != null)
                {
                    document = string.Format("{0}:Line {1}", seq.Document.Url, seq.StartLine);
                }
                sb.AppendFormat("at {0}.{1} {2}\r\n", m.DeclearingType.FullName, m.Definition.Name, document);
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
    }
}
