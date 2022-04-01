using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    internal sealed class AD7StepCompleteEvent : AD7StoppingEvent, IDebugStepCompleteEvent2
    {
        public const string IID = "0f7f24c1-74d9-4ea6-a3ea-7edb2d81441d";
    }
    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread is created in a program being debugged.
    internal sealed class AD7ThreadCreateEvent : AD7AsynchronousEvent, IDebugThreadCreateEvent2
    {
        public const string IID = "2090CCFC-70C5-491D-A5E8-BAD2DD9EE3EA";
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread has exited.
    internal sealed class AD7ThreadDestroyEvent : AD7AsynchronousEvent, IDebugThreadDestroyEvent2
    {
        public const string IID = "2C3B7532-A36F-4A6E-9072-49BE649B8541";

        private readonly uint _exitCode;
        public AD7ThreadDestroyEvent(uint exitCode)
        {
            _exitCode = exitCode;
        }

        #region IDebugThreadDestroyEvent2 Members

        int IDebugThreadDestroyEvent2.GetExitCode(out uint exitCode)
        {
            exitCode = _exitCode;

            return Constants.S_OK;
        }

        #endregion
    }

    internal sealed class AD7BreakpointErrorEvent : AD7AsynchronousEvent, IDebugBreakpointErrorEvent2
    {
        public const string IID = "ABB0CA42-F82B-4622-84E4-6903AE90F210";

        private AD7ErrorBreakpoint _errorBreakpoint;

        public AD7BreakpointErrorEvent(AD7ErrorBreakpoint errorBreakpoint = null)
        {
            _errorBreakpoint = errorBreakpoint;
        }

        #region IDebugBreakpointBoundEvent2 Members        

        public int GetErrorBreakpoint(out IDebugErrorBreakpoint2 ppErrorBP)
        {
            ppErrorBP = _errorBreakpoint;
            return Constants.S_OK;
        }

        #endregion
    }
    internal sealed class AD7BreakpointBoundEvent : AD7AsynchronousEvent, IDebugBreakpointBoundEvent2
    {
        public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private AD7PendingBreakPoint _pendingBreakpoint;
        private AD7BoundBreakpoint _boundBreakpoint;

        //public AD7BreakpointBoundEvent(AD7PendingBreakpoint pendingBreakpoint, AD7BoundBreakpoint boundBreakpoint)
        public AD7BreakpointBoundEvent(AD7PendingBreakPoint pendingBreakpoint, AD7BoundBreakpoint boundBreakpoint = null)
        {
            _pendingBreakpoint = pendingBreakpoint;
            _boundBreakpoint = boundBreakpoint;
        }

        #region IDebugBreakpointBoundEvent2 Members

        int IDebugBreakpointBoundEvent2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            //IDebugBoundBreakpoint2[] boundBreakpoints = new IDebugBoundBreakpoint2[1];
            //boundBreakpoints[0] = _boundBreakpoint;
            //ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            //return Constants.S_OK;

            return _pendingBreakpoint.EnumBoundBreakpoints(out ppEnum);
        }

        int IDebugBreakpointBoundEvent2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = _pendingBreakpoint;
            return Constants.S_OK;
        }

        #endregion
    }

    // This Event is sent when a breakpoint is hit in the debuggee
    //BreakPointHitEvent
    internal sealed class AD7BreakpointEvent : AD7StoppingEvent, IDebugBreakpointEvent2
    {
        public const string IID = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

        //AD7PendingBreakpoint
        //private IEnumDebugBoundBreakpoints2 _boundBreakpoints;
        private readonly AD7PendingBreakPoint _boundBreakpoints;

        public AD7BreakpointEvent(AD7PendingBreakPoint boundBreakpoints)
        {
            _boundBreakpoints = boundBreakpoints;
        }

        #region IDebugBreakpointEvent2 Members

        int IDebugBreakpointEvent2.EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = null;
            if (_boundBreakpoints == null)
                return Constants.S_OK;
            else
                return _boundBreakpoints.EnumBoundBreakpoints(out ppEnum);

            //ppEnum = _boundBreakpoints;
            //return Constants.S_OK;
        }

        #endregion
    }
    sealed class AD7ModuleLoadEvent : AD7AsynchronousEvent, IDebugModuleLoadEvent2
    {
        public const string IID = "989DB083-0D7C-40D1-A9D9-921BF611A4B2";

        readonly AD7Module m_module;
        readonly bool m_fLoad;

        public AD7ModuleLoadEvent(AD7Module module, bool fLoad)
        {
            m_module = module;
            m_fLoad = fLoad;
        }

        int IDebugModuleLoadEvent2.GetModule(out IDebugModule2 module, ref string debugMessage, ref int fIsLoad)
        {
            module = m_module;

            if (m_fLoad)
            {
                debugMessage = String.Concat("Loaded '", m_module.ModuleName, "'");
                fIsLoad = 1;
            }
            else
            {
                debugMessage = String.Concat("Unloaded '", m_module.ModuleName, "'");
                fIsLoad = 0;
            }

            return Constants.S_OK;
        }
    }
    internal sealed class AD7EngineCreateEvent : AD7AsynchronousEvent, IDebugEngineCreateEvent2
    {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";
        private IDebugEngine2 _engine;

        //private AD7EngineCreateEvent(AD7Engine engine)
        public AD7EngineCreateEvent(AD7Engine engine)
        {
            _engine = engine;
        }

        public static void Send(AD7Engine engine)
        {
            AD7EngineCreateEvent eventObject = new AD7EngineCreateEvent(engine);
            engine.Callback.Send(eventObject, IID, null, null);
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine)
        {
            engine = _engine;

            return Constants.S_OK;
        }
    }

    internal sealed class AD7ProgramCreateEvent : AD7SynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

        internal static void Send(AD7Engine engine)
        {
            AD7ProgramCreateEvent eventObject = new AD7ProgramCreateEvent();
            engine.Callback.Send(eventObject, IID, engine, null);
        }
    }

    sealed class AD7LoadCompleteEvent : AD7StoppingEvent, IDebugLoadCompleteEvent2
    {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";

        public AD7LoadCompleteEvent()
        {
        }
    }

    internal sealed class AD7ProgramDestroyEvent : AD7SynchronousEvent, IDebugProgramDestroyEvent2
    {
        public const string IID = "E147E9E3-6440-4073-A7B7-A65592C714B5";

        private readonly uint _exitCode;
        public AD7ProgramDestroyEvent(uint exitCode)
        {
            _exitCode = exitCode;
        }

        #region IDebugProgramDestroyEvent2 Members

        int IDebugProgramDestroyEvent2.GetExitCode(out uint exitCode)
        {
            exitCode = _exitCode;

            return Constants.S_OK;
        }

        #endregion
    }
    internal sealed class AD7EntryPointEvent : AD7StoppingEvent, IDebugEntryPointEvent2
    {
        public const string IID = "E8414A3E-1642-48EC-829E-5F4040E16DA9";

        public AD7EntryPointEvent()
        {
        }
    }
    internal class AD7AsynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return Constants.S_OK;
        }
    }
    class AD7SynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return Constants.S_OK;
        }
    }

    class AD7StoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return Constants.S_OK;
        }
    }
}
