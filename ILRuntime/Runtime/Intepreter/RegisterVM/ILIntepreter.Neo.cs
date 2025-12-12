using System;
using System.Collections.Generic;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Intepreter
{
	public unsafe partial class ILIntepreter
	{
		internal StackObject* ExecuteNeo(ILMethod method, StackObject* esp, out bool unhandledException)
		{

#if DEBUG
			if (method == null)
				throw new NullReferenceException();
#endif
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample(method.ToString());
#else
                UnityEngine.Profiler.BeginSample(method.ToString());
#endif

#endif
			OpCodeR[] body = method.BodyRegister;
			StackFrame frame;
			stack.InitializeFrame(method, esp, out frame);
			frame.IsRegister = true;
			int finallyEndAddress = 0;
			Exception lastCaughtEx = null;

			var stackRegStart = frame.LocalVarPointer;
			StackObject* r = frame.LocalVarPointer - method.ParameterCount;
			AutoList mStack = stack.ManagedStack;
			int paramCnt = method.ParameterCount;
			if (method.HasThis)//this parameter is always object reference
			{
				r--;
				paramCnt++;
				/// 为确保性能，暂时先确保开发的时候，安全检查完备。
				/// 当然手机端运行时可能会出现为空的类对象可正常调用成员函数，导致成员函数里面访问成员变量报错时可能使得根据Log跟踪BUG时方向错误。
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
				if (!method.DeclearingType.IsValueType)
				{
					var thisObj = RetriveObject(r, mStack);
					if (thisObj == null)
						throw new NullReferenceException();
				}
#endif
			}
			unhandledException = false;
			var hasReturn = method.ReturnType != AppDomain.VoidType;

			//Managed Stack reserved for arguments(In case of starg)
			for (int i = 0; i < paramCnt; i++)
			{
				var a = (r + i);
				switch (a->ObjectType)
				{
					/*case ObjectTypes.Null:
                        //Need to reserve place for null, in case of starg
                        a->ObjectType = ObjectTypes.Object;
                        a->Value = mStack.Count;
                        mStack.Add(null);
                        break;*/
					case ObjectTypes.ValueTypeObjectReference:
						//CloneStackValueType(a, a, mStack);
						break;
					case ObjectTypes.Object:
					case ObjectTypes.FieldReference:
					case ObjectTypes.ArrayReference:
						{
							if (i > 0 || !method.HasThis)//this instance should not be cloned
								mStack[a->Value] = CheckAndCloneValueType(mStack[a->Value], AppDomain);
						}
						break;
				}
			}
			frame.ManagedStackBase -= paramCnt;
			stack.PushFrame(ref frame);

			int locBase = mStack.Count;
			int locCnt = method.LocalVariableCount;
			int stackRegCnt = method.StackRegisterCount;
			RegisterFrameInfo info;
			info.Intepreter = this;
			info.StackBase = stack.StackBase;
			info.LocalManagedBase = locBase;
			info.FrameManagedBase = frame.ManagedStackBase;
			info.RegisterStart = r;
			info.StackRegisterStart = stackRegStart + locCnt;
			info.ManagedStack = mStack;

			object obj;

			/*for (int i = 0; i < locCnt; i++)
            {
                InitializeRegisterLocal(method, i, v1, locBase, mStack);
            }*/
			esp = stackRegStart + stackRegCnt + locCnt;

			info.RegisterEnd = esp;

			for (int i = 0; i < stackRegCnt + locCnt; i++)
			{
				var loc = stackRegStart + i;
				loc->ObjectType = ObjectTypes.Object;
				loc->Value = mStack.Count;
				mStack.Add(null);
			}
			var bp = stack.ValueTypeStackPointer;
			ValueTypeBasePointer = bp;
			var ehs = method.ExceptionHandlerRegister;

			StackObject* reg1, reg2, reg3, objRef, objRef2, val, dst, ret;
			ILTypeInstance ilInstance;
			bool transfer;
			int intVal = 0;
			long longVal = 0;
			float floatVal = 0;
			double doubleVal = 0;
			IType type;
			CLRMethod cm;
			Type clrType;
			IMethod m;

			fixed (OpCodeR* ptr = body)
			{
				OpCodeR* ip = ptr;
				OpCodeREnum code = ip->Code;
				bool returned = false;
				while (!returned)
				{
					try
					{
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
						if (ShouldBreak)
							Break();
						var insOffset = (int)(ip - ptr);
						frame.Address.Value = insOffset;
						AppDomain.DebugService.CheckShouldBreak(method, this, insOffset);
#endif
						code = ip->Code;
						switch (code)
						{
							case OpCodeREnum.Ret:
								if (hasReturn)
								{
									reg1 = (r + ip->Register1);
									CopyToStack(esp, reg1, mStack);
									esp++;
								}
								returned = true;
								break;
						}
						ip++;
					}
					catch(Exception ex)
					{
						var oriESP = esp;
						bool isJmp = HandleException(ex, ref esp, ehs, method, (int)(ip - ptr), ref frame, ref lastCaughtEx, ref unhandledException, ref finallyEndAddress, out int jmpTarget, out bool isCatch);
						if (isCatch)
						{
							short exReg = (short)(paramCnt + locCnt);
							AssignToRegister(ref info, exReg, ex);
						}
						if (isJmp)
						{
							ip = ptr + jmpTarget;
							continue;
						}
						if (unhandledException)
						{
							throw ex;
						}

						unhandledException = true;
						returned = true;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
						if (!AppDomain.DebugService.Break(this, ex))
#endif
						{
							var newEx = new ILRuntimeException(ex.Message, this, method, oriESP, ex);
							throw newEx;
						}
					}
				}
			}
		}
	}
}
