using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using ILRuntimeDebugEngine.Utils;

namespace ILRuntimeDebugEngine.AD7
{
    class EngineCallback
    {
        private readonly IDebugEventCallback2 _eventCallback;
        private readonly AD7Engine _engine;
        

        public EngineCallback(AD7Engine engine, IDebugEventCallback2 pCallback)
        {
            _engine = engine;
            _eventCallback = pCallback;
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes;
            Guid riidEvent = new Guid(iidEvent);

            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));
            EngineUtils.RequireOk(_eventCallback.Event(_engine, null, program, thread, eventObject, ref riidEvent, attributes));
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugThread2 thread)
        {
            IDebugProgram2 program = _engine;
            if (!_engine.ProgramCreateEventSent)
            {
                // Any events before programe create shouldn't include the program
                program = null;
            }

            Send(eventObject, iidEvent, program, thread);
        }

        public void EngineCreated()
        {
            var iid = new Guid(AD7EngineCreateEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7EngineCreateEvent(_engine), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        public void ProgramCreated()
        {
            var iid = new Guid(AD7ProgramCreateEvent.IID);
            _eventCallback.Event(_engine, null, _engine, null, new AD7ProgramCreateEvent(), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        public void EngineLoaded()
        {
            var iid = new Guid(AD7LoadCompleteEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7LoadCompleteEvent(), ref iid,
                AD7StoppingEvent.Attributes);
        }

        internal void DebugEntryPoint()
        {
            var iid = new Guid(AD7EntryPointEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7EntryPointEvent(), ref iid, AD7AsynchronousEvent.Attributes);
        }

        internal void ProgramDestroyed(IDebugProgram2 program)
        {
            var iid = new Guid(AD7ProgramDestroyEvent.IID);
            _eventCallback.Event(_engine, null, program, null, new AD7ProgramDestroyEvent(0), ref iid, AD7AsynchronousEvent.Attributes);
        }

        internal void BoundBreakpoint(AD7PendingBreakPoint breakpoint)
        {
            var iid = new Guid(AD7BreakpointBoundEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7BreakpointBoundEvent(breakpoint), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        internal void ErrorBreakpoint(AD7ErrorBreakpoint breakpoint)
        {
            var iid = new Guid(AD7BreakpointErrorEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7BreakpointErrorEvent(breakpoint), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        internal void ModuleLoaded(AD7Module module)
        {
            var iid = new Guid(AD7ModuleLoadEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, null, new AD7ModuleLoadEvent(module, true), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        internal void BreakpointHit(AD7PendingBreakPoint breakpoint, AD7Thread thread)
        {
            var iid = new Guid(AD7BreakpointEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, thread, new AD7BreakpointEvent(breakpoint), ref iid,
                AD7StoppingEvent.Attributes);
        }

        internal void ThreadStarted(AD7Thread thread)
        {
            var iid = new Guid(AD7ThreadCreateEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, thread, new AD7ThreadCreateEvent(), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        internal void ThreadEnded(AD7Thread thread)
        {
            var iid = new Guid(AD7ThreadDestroyEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, thread, new AD7ThreadDestroyEvent(0), ref iid,
                AD7AsynchronousEvent.Attributes);
        }

        internal void StepCompleted(AD7Thread thread)
        {
            var iid = new Guid(AD7StepCompleteEvent.IID);
            _eventCallback.Event(_engine, _engine.RemoteProcess, _engine, thread, new AD7StepCompleteEvent(), ref iid,
                AD7StoppingEvent.Attributes);
        }

        /*public void OnError(string message)
        {
            SendMessage(message, OutputMessage.Severity.Error, isAsync: true);
        }

        /// <summary>
        /// Sends an error to the user, blocking until the user dismisses the error
        /// </summary>
        /// <param name="message">string to display to the user</param>
        public void OnErrorImmediate(string message)
        {
            SendMessage(message, OutputMessage.Severity.Error, isAsync: false);
        }

        private void SendMessage(string message, OutputMessage.Severity severity, bool isAsync)
        {
            try
            {
                // IDebugErrorEvent2 is used to report error messages to the user when something goes wrong in the debug engine.
                // The sample engine doesn't take advantage of this.

                AD7MessageEvent eventObject = new AD7MessageEvent(new OutputMessage(message, enum_MESSAGETYPE.MT_MESSAGEBOX, severity), isAsync);
                Send(eventObject, AD7MessageEvent.IID, null);
            }
            catch
            {
                // Since we are often trying to report an exception, if something goes wrong we don't want to take down the process,
                // so ignore the failure.
            }
        }*/

        //public void OnWarning(string message)
        //{
        //    SendMessage(message, OutputMessage.Severity.Warning, isAsync: true);
        //}

        //public void OnCustomDebugEvent(Guid guidVSService, Guid sourceId, int messageCode, object parameter1, object parameter2)
        //{
        //    var eventObject = new AD7CustomDebugEvent(guidVSService, sourceId, messageCode, parameter1, parameter2);
        //    Send(eventObject, AD7CustomDebugEvent.IID, null);
        //}
    }
}
