using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    class AsyncJITCompileWorker
    {
        AutoResetEvent evt = new AutoResetEvent(false);
        Queue<ILMethod> jobs = new Queue<ILMethod>();
        bool exit;
        Thread thread;
        public AsyncJITCompileWorker()
        {
            thread = new Thread(DoJob);
            thread.Name = "ILRuntime JIT Worker";
            thread.Start();
        }
        public void QueueCompileJob(ILMethod method)
        {
            if (exit)
                throw new NotSupportedException("Already disposed");
            lock (jobs)
                jobs.Enqueue(method);
            evt.Set();
        }

        public void Dispose()
        {
            exit = true;
        }
        void DoJob()
        {
            while (!exit)
            {
                evt.WaitOne();
                while (jobs.Count > 0)
                {
                    ILMethod m;
                    lock (jobs)
                        m = jobs.Dequeue();
                    m.InitCodeBody(true);
                }
            }
        }
    }
}
