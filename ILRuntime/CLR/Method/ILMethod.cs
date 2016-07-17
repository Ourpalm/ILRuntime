using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        ILType declaringType;
        ExceptionHandler[] exceptionHandler;

        public MethodDefinition Definition { get { return def; } }

        public ExceptionHandler[] ExceptionHandler
        {
            get
            {
                if (body == null)
                    InitCodeBody(); 
                return exceptionHandler;
            }
        }

        public string Name
        {
            get
            {
                return def.Name;
            }
        }

        public IType DeclearingType
        {
            get
            {
                return declaringType;
            }
        }

        public bool HasThis
        {
            get
            {
                return def.HasThis;
            }
        }
        public int GenericParameterCount
        {
            get
            {
                return 0;
            }
        }
        public bool IsGenericInstance
        {
            get
            {
                return false;
            }
        }
        public ILMethod(MethodDefinition def, ILType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.def = def;
            declaringType = type;
            ReturnType = domain.GetType(def.ReturnType.FullName);
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

        public bool IsConstructor
        {
            get
            {
                return def.IsConstructor;
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

        public IType ReturnType
        {
            get;
            private set;
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

                for (int i = 0; i < def.Body.ExceptionHandlers.Count; i++)
                {
                    var eh = def.Body.ExceptionHandlers[i];
                    if (exceptionHandler == null)
                        exceptionHandler = new Method.ExceptionHandler[def.Body.ExceptionHandlers.Count];
                    ExceptionHandler e = new ExceptionHandler();
                    e.HandlerStart = addr[eh.HandlerStart];
                    e.HandlerEnd = addr[eh.HandlerEnd] - 1;
                    e.TryStart = addr[eh.TryStart];
                    e.TryEnd = addr[eh.TryEnd] - 1;
                    switch (eh.HandlerType)
                    {
                        case Mono.Cecil.Cil.ExceptionHandlerType.Catch:
                            e.CatchType = appdomain.GetType(eh.CatchType, declaringType);
                            e.HandlerType = ExceptionHandlerType.Catch;
                            break;
                        case Mono.Cecil.Cil.ExceptionHandlerType.Finally:
                            e.HandlerType = ExceptionHandlerType.Finally;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    exceptionHandler[i] = e;
                    //Mono.Cecil.Cil.ExceptionHandlerType.
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
                    code.TokenInteger = addr[(Mono.Cecil.Cil.Instruction)token]; 
                    break;
                case OpCodeEnum.Ldc_I4:
                    code.TokenInteger = (int)token;
                    break;
                case OpCodeEnum.Ldc_I4_S:
                    code.TokenInteger = (sbyte)token;
                    break;
                case OpCodeEnum.Ldc_I8:
                    code.TokenLong = (long)token;
                    break;
                case OpCodeEnum.Ldc_R4:
                    code.TokenFloat = (float)token;
                    break;
                case OpCodeEnum.Ldc_R8:
                    code.TokenDouble = (double)token;
                    break;                    
                case OpCodeEnum.Stloc:
                case OpCodeEnum.Stloc_S:
                case OpCodeEnum.Ldloc:
                case OpCodeEnum.Ldloc_S:
                case OpCodeEnum.Ldloca:
                case OpCodeEnum.Ldloca_S:
                    {
                        Mono.Cecil.Cil.VariableDefinition vd = (Mono.Cecil.Cil.VariableDefinition)token;
                        code.TokenInteger = vd.Index;
                    }
                    break;
                case OpCodeEnum.Ldarg_S:
                case OpCodeEnum.Ldarg:
                case OpCodeEnum.Ldarga:
                case OpCodeEnum.Ldarga_S:
                    {
                        Mono.Cecil.ParameterDefinition vd = (Mono.Cecil.ParameterDefinition)token;
                        code.TokenInteger = vd.Index;
                    }
                    break;
                case OpCodeEnum.Call:
                case OpCodeEnum.Newobj:
                case OpCodeEnum.Callvirt:
                    {
                        var m = appdomain.GetMethod(token, declaringType);
                        if (m != null)
                            code.TokenInteger = token.GetHashCode();
                    }
                    break;
                case OpCodeEnum.Box:
                case OpCodeEnum.Initobj:
                case OpCodeEnum.Isinst:
                case OpCodeEnum.Newarr:
                    {
                        var t = appdomain.GetType(token, declaringType);
                        if (t != null)
                            code.TokenInteger = token.GetHashCode();
                    }
                    break;
                case OpCodeEnum.Stfld:
                case OpCodeEnum.Ldfld:
                case OpCodeEnum.Ldflda:
                    {
                        code.TokenInteger = appdomain.GetFieldIndex(token, declaringType);   
                    }
                    break;

                case OpCodeEnum.Stsfld:
                case OpCodeEnum.Ldsfld:
                case OpCodeEnum.Ldsflda:
                    {
                        code.TokenLong = appdomain.GetStaticFieldIndex(token, declaringType);   
                    }
                    break;
                case OpCodeEnum.Ldstr:
                    {
                        int hashCode = token.GetHashCode();
                        appdomain.CacheString(token);
                        code.TokenInteger = hashCode;
                    }
                    break;
                case OpCodeEnum.Ldtoken:
                    {
                        if (token is FieldReference)
                        {
                            code.TokenInteger = 0;
                            code.TokenLong = appdomain.GetStaticFieldIndex(token, declaringType);
                        }
                        else
                            throw new NotImplementedException();
                    }
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

        public IMethod MakeGenericMethod(IType[] genericArguments)
        {
            throw new NotImplementedException();
        }
    }
}
