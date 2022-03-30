using ILRuntime.Runtime.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ILRuntimeDebugEngine.AD7
{
    class DebuggedProcessVS : DebuggedProcess
    {
        AD7Engine engine;

        public DebuggedProcessVS(AD7Engine engine, string host, int port)
            : base(host, port)
        {
            this.engine = engine;
        }

        protected override IProperty CreateProperty(IStackFrame frame, VariableInfo info)
        {
            return new ILProperty(engine, ((AD7StackFrame)frame).Thread, info, (AD7StackFrame)frame);
               
        }

        protected override void HandleBreakpointHit(IBreakPoint bp, IThread bpThread)
        {
            engine.Callback.BreakpointHit((AD7PendingBreakPoint)bp, (AD7Thread)bpThread);
        }

        protected override void HandleModuleLoaded(string moduleName)
        {
            engine.Callback.ModuleLoaded(new AD7Module(moduleName));

        }

        protected override void HandleShowErrorMessageBox(string errorMsg)
        {
            if (AD7Engine.ShowErrorMessageBoxAction != null)
                AD7Engine.ShowErrorMessageBoxAction("ILRuntime Debugger", errorMsg);
            else
                MessageBox.Show(errorMsg, "ILRuntime Debugger", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void HandleStepCompelte(IThread bpThread)
        {
            engine.Callback.StepCompleted((AD7Thread)bpThread);
        }

        protected override IThread HandleTheadStarted(int threadHash)
        {
            AD7Thread t = new AD7Thread(engine, threadHash);
            engine.Callback.ThreadStarted(t);
            return t;
        }

        protected override void HandleThreadEnded(IThread t)
        {
            engine.Callback.ThreadEnded((AD7Thread)t);
        }
    }
}
