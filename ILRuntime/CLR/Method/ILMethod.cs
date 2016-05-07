using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
namespace ILRuntime.CLR.Method
{
    class ILMethod : IMethod
    {
        OpCode[] body;
        MethodDefinition def;
        public ILMethod(MethodDefinition def)
        {
            this.def = def;
        }

        public OpCode[] Body
        {
            get
            {
                if (body == null)
                    InitCodeBody();
                return body;
            }
        }

        public int LocalVariableCount
        {
            get
            {
                return def.HasBody ? def.Body.Variables.Count : 0;
            }
        }

        public int ParameterCount
        {
            get
            {
                return def.HasParameters ? def.Parameters.Count : 0;
            }
        }

        void InitCodeBody()
        {
            if (def.HasBody)
            {
                body = new OpCode[def.Body.Instructions.Count];
                for(int i = 0; i < body.Length; i++)
                {
                    var c = def.Body.Instructions[i];
                    OpCode code = new OpCode();
                    code.Code = (OpCodeEnum)c.OpCode.Code;
                    body[i] = code;
                }
            }
            else
                body = new OpCode[0];
        }
    }
}
