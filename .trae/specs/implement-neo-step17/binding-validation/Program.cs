using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using ILRuntime.Runtime.CLRBinding;
using ILRuntimeTest.TestFramework;
class Program
{
    static void Main(string[] args)
    {
        if (args[0] == "generate")
        {
            BindingCodeGenerator.GenerateBindingCode(new List<Type> { typeof(NeoStep17CLRHost) }, args[1]);
            return;
        }
        var app = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.JITNeo);
        using var stream = File.OpenRead(args[1]);
        app.LoadAssembly(stream);
        if (args[0] == "bench")
        {
            // End-to-end reference workloads, including Invoke overhead. No generated bindings.
            foreach (string method in new[] { "NeoStep17HeapAndStatic", "NeoStep17StructFields", "NeoStep17Arrays", "NeoStep17ClrWholeFields", "NeoStep17ClrNested" })
            {
                var target = app.LoadedTypes["TestCases.NeoStep17Test"].GetMethod(method, 0);
                for (int i = 0; i < 2000; i++) app.Invoke(target, null, Array.Empty<object>());
                var samples = new double[9];
                long allocated = 0;
                for (int sample = 0; sample < samples.Length; sample++)
                {
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    var timer = Stopwatch.StartNew();
                    for (int i = 0; i < 10000; i++) app.Invoke(target, null, Array.Empty<object>());
                    timer.Stop();
                    allocated += GC.GetAllocatedBytesForCurrentThread() - before;
                    samples[sample] = timer.Elapsed.TotalMilliseconds;
                }
                Array.Sort(samples);
                Console.WriteLine($"BENCH {method}: median={samples[4]:F3} ms/10000, allocation={allocated / 90000.0:F1} B/invoke");
            }
            return;
        }
        Assembly.GetExecutingAssembly().GetType("ILRuntime.Runtime.Generated.CLRBindings").GetMethod("Initialize").Invoke(null, new object[] { app });
        foreach (string method in new[] { "NeoStep17ClrRefOut", "NeoStep17ClrNested", "NeoStep17ClrStructRef", "NeoStep17ClrNarrowRefs", "NeoStep17ClrWholeFields" })
        {
            app.Invoke("TestCases.NeoStep17Test", method, null, new object[0]);
            Console.WriteLine("PASS generated: " + method);
        }
    }
}
