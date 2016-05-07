using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.CLR.TypeSystem;
namespace ILRuntime.CLR.Method
{
    class ILMethod : IMethod
    {
        OpCode[] body;
        MethodDefinition def;
        List<IType> parameters;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        public ILMethod(MethodDefinition def, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.def = def;
            this.appdomain = domain;
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


        public List<IType> Parameters
        {
            get
            {
                if (def.HasParameters && parameters == null)
                {
                    InitParameters();
                }
                return parameters;
            }
        }
        void InitCodeBody()
        {
            if (def.HasBody)
            {
                body = new OpCode[def.Body.Instructions.Count];
                Dictionary<Mono.Cecil.Cil.Instruction, int> addr = new Dictionary<Mono.Cecil.Cil.Instruction, int>();
                for (int i = 0; i < body.Length; i++)
                {
                    var c = def.Body.Instructions[i];
                    OpCode code = new OpCode();
                    code.Code = (OpCodeEnum)c.OpCode.Code;
                    addr[c] = i;
                    body[i] = code;
                }
                for (int i = 0; i < body.Length; i++)
                {
                    var c = def.Body.Instructions[i];
                    InitToken(ref body[i], c.Operand, addr);
                }
            }
            else
                body = new OpCode[0];
        }

        void InitToken(ref OpCode code, object token, Dictionary<Mono.Cecil.Cil.Instruction, int> addr)
        {
            switch (code.Code)
            {
                case OpCodeEnum.Leave:
                case OpCodeEnum.Leave_S:
                case OpCodeEnum.Br:
                case OpCodeEnum.Br_S:
                case OpCodeEnum.Brtrue:
                case OpCodeEnum.Brtrue_S:
                case OpCodeEnum.Brfalse:
                case OpCodeEnum.Brfalse_S:
                //比较流程控制
                case OpCodeEnum.Beq:
                case OpCodeEnum.Beq_S:
                case OpCodeEnum.Bne_Un:
                case OpCodeEnum.Bne_Un_S:
                case OpCodeEnum.Bge:
                case OpCodeEnum.Bge_S:
                case OpCodeEnum.Bge_Un:
                case OpCodeEnum.Bge_Un_S:
                case OpCodeEnum.Bgt:
                case OpCodeEnum.Bgt_S:
                case OpCodeEnum.Bgt_Un:
                case OpCodeEnum.Bgt_Un_S:
                case OpCodeEnum.Ble:
                case OpCodeEnum.Ble_S:
                case OpCodeEnum.Ble_Un:
                case OpCodeEnum.Ble_Un_S:
                case OpCodeEnum.Blt:
                case OpCodeEnum.Blt_S:
                case OpCodeEnum.Blt_Un:
                case OpCodeEnum.Blt_Un_S:
                    code.TokenAddress = addr[(Mono.Cecil.Cil.Instruction)token]; 
                    break;
            }
        }

        void InitParameters()
        {
            parameters = new List<IType>();
            foreach (var i in def.Parameters)
            {
                IType type = appdomain.GetType(i.ParameterType.FullName);
                parameters.Add(type);
            }
        }
    }
}
