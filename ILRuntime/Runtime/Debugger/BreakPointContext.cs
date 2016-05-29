using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Debugger
{
    unsafe class BreakPointContext
    {
        public ILIntepreter Interpreter { get; set; }
        public Exception Exception { get; set; }

        public string DumpContext()
        {
            StringBuilder sb = new StringBuilder();
            if (Exception != null)
                sb.AppendLine(Exception.Message);
            StackFrame[] frames = Interpreter.Stack.Frames.ToArray();
            StackFrame topFrame = frames[0];
            var m = topFrame.Method;
            if (m.HasThis)
            {
                sb.AppendLine("this:");
                var arg = topFrame.LocalVarPointer - 1;
                ILTypeInstance instance = Interpreter.Stack.ManagedStack[arg->Value] as ILTypeInstance;
                var fields = instance.Type.TypeDefinition.Fields;
                int idx = 0;
                for(int i = 0; i < fields.Count; i++)
                {
                    var f = fields[i];
                    if (f.IsStatic)
                        continue;                    
                    var v = GetStackObjectValue(instance.Fields[idx], instance.ManagedObjects);
                    string name = f.Name;
                    sb.AppendFormat("{0} {1} = {2}", f.FieldType.Name, name, v);
                    if ((idx % 3 == 0 && idx != 0) || idx == instance.Fields.Length - 1)
                        sb.AppendLine();
                    else
                        sb.Append(", ");
                    idx++;
                }
            }
            sb.AppendLine("->" + topFrame.Method.Definition.Body.Instructions[topFrame.Address.Value]);
            sb.AppendLine("Local Variables:");
            for (int i = 0; i < m.LocalVariableCount; i++)
            {
                var lv = m.Definition.Body.Variables[i];
                var val = topFrame.LocalVarPointer + i;
                var v = GetStackObjectValue(*val, Interpreter.Stack.ManagedStack);
                string name = string.IsNullOrEmpty(lv.Name) ? "v" + lv.Index : lv.Name;
                sb.AppendFormat("{0} {1} = {2}", lv.VariableType.Name, name, v);
                if ((i % 3 == 0 && i != 0) || i == m.LocalVariableCount - 1)
                    sb.AppendLine();
                else
                    sb.Append(", ");
            }

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
                sb.AppendFormat("at {0}.{1} {2}\n", m.DeclearingType.FullName, m.Definition.Name, document);
            }
            return sb.ToString();
        }

        string GetStackObjectValue(StackObject val, List<object> mStack)
        {
            string v;
            switch (val.ObjectType)
            {
                case ObjectTypes.Null:
                    v = "null";
                    break;
                case ObjectTypes.Integer:
                    v = val.Value.ToString();
                    break;
                case ObjectTypes.Object:
                    {
                        object obj = Interpreter.Stack.ManagedStack[val.Value];
                        v = obj.ToString();
                    }
                    break;
                default:
                    v = "Unknown type";
                    break;
            }
            return v;
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
