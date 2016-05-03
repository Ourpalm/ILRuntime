using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter
{
    class ILIntepreter
    {
        Enviorment.AppDomain domain;
        RuntimeStack stack;

        public ILIntepreter(Enviorment.AppDomain domain)
        {
            this.domain = domain;
            stack = new RuntimeStack(this);
        }

        public unsafe void Run(ILMethod method)
        {
            OpCode[] body = method.Body;
            StackObject v1 = new StackObject();
            StackObject v2 = new StackObject();
            StackObject v3 = new StackObject();
            StackObject v4 = new StackObject();
            StackObject v5 = new StackObject();

            StackObject* stackBase = stack.StackBase;
            StackObject* esp = stackBase;
            StackObject* paramPtr;
            StackObject* localVars = esp;
            //esp += var count;
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;

            fixed (OpCode* ptr = body)
            {
                OpCode* ip = ptr;
                OpCodeEnum code = ip->Code;
                bool returned = false;
                while (!returned)
                {
                    code = ip->Code;
                    switch (code)
                    {
                        case OpCodeEnum.Stloc_0:
                            esp--;
                            v1 = *esp;
                            break;
                        case OpCodeEnum.Ldloc_0:
                            *esp = v1;
                            esp++;
                            break;
                        case OpCodeEnum.Stloc_1:
                            esp--;
                            v2 = *esp;
                            break;
                        case OpCodeEnum.Ldloc_1:
                            *esp = v2;
                            esp++;
                            break;
                        case OpCodeEnum.Ldc_I4_0:
                            esp->Value = 0;
                            esp->ObjectType =  ObjectTypes.Integer;
                            esp++;
                            break;
                        case OpCodeEnum.Add:
                            {
                                StackObject* a = esp - 1;
                                StackObject* b = esp - 2;
                                esp -= 2;
                                if(a->ObjectType == ObjectTypes.Long)
                                {
                                    esp->ObjectType = ObjectTypes.Long;
                                    *((long*)&esp->Value) = *((long*)&a->Value) + *((long*)&b->Value);
                                }
                                else
                                {
                                    esp->ObjectType = ObjectTypes.Integer;
                                    esp->Value = a->Value + b->Value;
                                }
                            }
                            break;
                        case OpCodeEnum.Ldind_I:
                            break;
                        case OpCodeEnum.Ldind_I1:
                            break;
                        case OpCodeEnum.Ldind_I2:
                            break;
                        case OpCodeEnum.Ldind_I4:
                            break;
                        case OpCodeEnum.Ldind_I8:
                            break;
                        case OpCodeEnum.Ret:
                            returned = true;
                            break;
                    }
                    ip++;
                }

                //ClearStack
                mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
            }
        }

    }
}
