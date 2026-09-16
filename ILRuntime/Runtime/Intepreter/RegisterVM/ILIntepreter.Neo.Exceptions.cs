#if ENABLE_NEO_MODE
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using ILRuntime.CLR.Method;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILRuntime.Runtime.Intepreter
{
    public unsafe partial class ILIntepreter
    {
        // Per invocation, allocated only when an EH transfer actually occurs. A finally can
        // execute another try/catch/finally without overwriting its suspended continuation.
        sealed class NeoExceptionState
        {
            sealed class Continuation
            {
                public int Target;
                public ExceptionHandler Catch;
                public ExceptionDispatchInfo Exception;
                public List<ExceptionHandler> Finalies;
                public int Next;
                public ExceptionHandler Running;
            }

            struct CatchContext
            {
                public ExceptionHandler Handler;
                public ExceptionDispatchInfo Exception;
            }

            readonly ILIntepreter owner;
            readonly ExceptionHandler[] handlers;
            readonly List<Continuation> pending = new List<Continuation>();
            readonly List<CatchContext> catches = new List<CatchContext>();
            public ExceptionHandler ReadyCatch { get; private set; }
            public ExceptionDispatchInfo Exception { get; private set; }

            public NeoExceptionState(ILIntepreter owner, ExceptionHandler[] handlers)
            {
                this.owner = owner;
                this.handlers = handlers ?? new ExceptionHandler[0];
            }

            static bool InTry(ExceptionHandler h, int address)
                => address >= h.TryStart && address <= h.TryEnd;
            static bool InHandler(ExceptionHandler h, int address)
                => address >= h.HandlerStart && address <= h.HandlerEnd;

            // Equal try ranges preserve metadata order; a narrower enclosing range wins.
            static int CompareRegion(ExceptionHandler a, ExceptionHandler b)
            {
                int start = b.TryStart.CompareTo(a.TryStart);
                return start != 0 ? start : a.TryEnd.CompareTo(b.TryEnd);
            }

            public int Raise(ExceptionDispatchInfo exception, int address)
            {
                ExceptionHandler selected = null;
                foreach (var h in handlers)
                    if (h.HandlerType == ExceptionHandlerType.Catch && InTry(h, address) &&
                        owner.CheckExceptionType(h.CatchType, exception.SourceException, false) &&
                        (selected == null || CompareRegion(h, selected) < 0))
                        selected = h;
                return Transfer(address, selected == null ? -1 : selected.HandlerStart, selected, exception);
            }

            public int Leave(int address, int target) => Transfer(address, target, null, null);

            public ExceptionDispatchInfo Rethrow(int address)
            {
                for (int i = catches.Count - 1; i >= 0; i--)
                    if (InHandler(catches[i].Handler, address)) return catches[i].Exception;
                throw new InvalidProgramException("Neo rethrow outside an active catch handler.");
            }

            int Transfer(int address, int target, ExceptionHandler handler, ExceptionDispatchInfo exception)
            {
                ReadyCatch = null;
                Exception = null;
                // A transfer caught within the running finally preserves its continuation.
                // Escaping that finally replaces the old leave/exception completely.
                while (pending.Count > 0 && !InHandler(pending[pending.Count - 1].Running, target))
                    pending.RemoveAt(pending.Count - 1);
                var finalies = new List<ExceptionHandler>();
                foreach (var h in handlers)
                    if (h.HandlerType == ExceptionHandlerType.Finally && InTry(h, address) && !InTry(h, target))
                        finalies.Add(h);
                finalies.Sort(CompareRegion);
                var continuation = new Continuation { Target = target, Catch = handler, Exception = exception, Finalies = finalies };
                if (finalies.Count > 0) pending.Add(continuation);
                return Advance(continuation);
            }

            int Advance(Continuation continuation)
            {
                ReadyCatch = null;
                Exception = null;
                if (continuation.Next < continuation.Finalies.Count)
                {
                    continuation.Running = continuation.Finalies[continuation.Next++];
                    return continuation.Running.HandlerStart;
                }
                if (pending.Count > 0 && ReferenceEquals(pending[pending.Count - 1], continuation))
                    pending.RemoveAt(pending.Count - 1);
                for (int i = catches.Count - 1; i >= 0; i--)
                    if (!InHandler(catches[i].Handler, continuation.Target)) catches.RemoveAt(i);
                ReadyCatch = continuation.Catch;
                Exception = continuation.Exception;
                if (ReadyCatch != null)
                    catches.Add(new CatchContext { Handler = ReadyCatch, Exception = Exception });
                return continuation.Target;
            }

            public int EndFinally(int address)
            {
                if (pending.Count == 0 || !InHandler(pending[pending.Count - 1].Running, address))
                    throw new InvalidProgramException("Neo endfinally without an active continuation.");
                return Advance(pending[pending.Count - 1]);
            }
        }

        void EnterNeoExceptionTarget(NeoExceptionState state, int frameDepth, byte* frameBase,
            int frameRefBase, int totalRefSize, AutoList mStack)
        {
            // Never use RuntimeStack.PopFrame here: it interprets memory as StackObject[].
            while (stack.Frames.Count > frameDepth) stack.Frames.Pop();
            int count = frameRefBase + totalRefSize;
            if (mStack.Count > count) mStack.RemoveRange(count, mStack.Count - count);
            var handler = state.ReadyCatch;
            if (handler != null)
            {
                int index = frameRefBase + handler.ExceptionRefOffset;
                mStack[index] = state.Exception.SourceException;
                *(int*)(frameBase + handler.ExceptionOffset) = index;
            }
        }

        static void RecordNeoException(Exception exception, ILMethod method, int address)
        {
            // Preserve the original exception and CLR stack; Neo frames are byte layouts,
            // so the Legacy local-variable/ThisInfo decoders must not inspect them.
            string location = method + " (instruction " + address + ")";
            object previous = exception.Data["StackTrace"];
            exception.Data["StackTrace"] = previous == null ? location : previous + "\n" + location;
        }
    }
}
#endif
