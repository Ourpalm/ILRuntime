using CommandLine;
using ILRuntime.Hybrid;
using System;
using System.IO;

namespace PatchTool
{
    internal class Program
    {
        class Options
        {
            [Value(0, Required = true, HelpText = "Input file")]
            public string File { get; set; }

            [Value(1, HelpText = "Patched assembly")]
            public string PatchedFile { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output file")]
            public string OutputFile { get; set; }

            [Option('h', "hash-file", HelpText ="Save path for hash info file")]
            public string HashFile { get; set; }


            [Option('p', "patch", SetName ="function", HelpText = "Generate patch file by providing the original file and the modified file")]
            public bool ShouldGeneratePatch { get; set; }
            [Option('i',"inject", SetName = "function", HelpText ="Inject the necessary code to support runtime patching")]
            public bool ShouldInject { get; set; }
        }
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args).MapResult(Run, _ => 1);
        }

        static int Run(Options options)
        {
            if (options.ShouldInject)
            {
                if (File.Exists(options.File))
                {
                    using (Stream fs = File.Open(options.File, FileMode.Open, FileAccess.Read))
                    {
                        var injector = AssemblyInjector.CreateInjector(fs);
                        injector.Inject();

                        using(Stream output = File.Create(options.OutputFile))
                        {
                            injector.WriteAssemblyToFile(output);
                        }
                        if (!string.IsNullOrEmpty(options.HashFile))
                        {
                            using (Stream output = File.Create(options.HashFile))
                            {
                                injector.WritePatchInfoToFile(output);
                            }
                        }
                    }
                    return 0;
                }
                else
                {
                    Console.WriteLine("File not exist:" + options.File);
                    return 1;
                }
            }
            else if (options.ShouldGeneratePatch)
            {
                if (string.IsNullOrEmpty(options.PatchedFile))
                {
                    Console.WriteLine("Please specify the patched version of the original assembly");
                    return 1;
                }
                if (File.Exists(options.File) && File.Exists(options.PatchedFile))
                {
                    using (Stream fs = File.Open(options.File, FileMode.Open, FileAccess.Read))
                    {
                        using (Stream fs2 = File.Open(options.PatchedFile, FileMode.Open, FileAccess.Read))
                        {
                            PatchGenerator generator = new PatchGenerator(fs, fs2);
                            generator.Analyze();

                            using (Stream output = File.Create(options.OutputFile))
                            {
                                generator.SavePatch(output);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The specified files do not exist!");
                }
                return 0;
            }
            else
            {
                Console.WriteLine("Must specify at least one function(-i or -p)");
                return 1;
            }
        }
    }
}
