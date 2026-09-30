namespace ILRuntimeTest.TestFramework
{
    public struct NeoStep17Inner { public int Number; public string Text; }
    public struct NeoStep17Outer { public NeoStep17Inner Inner; }
    public class NeoStep17CLRHost
    {
        public int Number;
        public string Text;
        public NeoStep17Outer Value;
        public static int StaticNumber;
        public static void Change(ref int number, out string text) { number += 5; text = "clr"; }
        public static void ChangeStruct(ref NeoStep17Inner value) { value.Number = 21; value.Text = "struct"; }
        public static void ChangeNarrow(ref byte value, ref short other) { value = 255; other = -12; }
        public static void CheckDanglingReturn()
        {
#if ENABLE_NEO_MODE
            using (var module = ILRuntime.Mono.Cecil.ModuleDefinition.CreateModule("NeoStep17Invalid", ILRuntime.Mono.Cecil.ModuleKind.Dll))
            using (var bytes = new System.IO.MemoryStream())
            {
                var type = new ILRuntime.Mono.Cecil.TypeDefinition("Tests", "Pointers",
                    ILRuntime.Mono.Cecil.TypeAttributes.Public | ILRuntime.Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
                module.Types.Add(type);
                var method = new ILRuntime.Mono.Cecil.MethodDefinition("Escape",
                    ILRuntime.Mono.Cecil.MethodAttributes.Public | ILRuntime.Mono.Cecil.MethodAttributes.Static,
                    new ILRuntime.Mono.Cecil.ByReferenceType(module.TypeSystem.Int32));
                type.Methods.Add(method);
                method.Body.Variables.Add(new ILRuntime.Mono.Cecil.Cil.VariableDefinition(module.TypeSystem.Int32));
                var il = method.Body.GetILProcessor();
                il.Emit(ILRuntime.Mono.Cecil.Cil.OpCodes.Ldloca, method.Body.Variables[0]);
                il.Emit(ILRuntime.Mono.Cecil.Cil.OpCodes.Ret);
                module.Write(bytes); bytes.Position = 0;
                var app = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.JITNeo);
                app.LoadAssembly(bytes);
                try { app.Invoke("Tests.Pointers", "Escape", null, new object[0]); }
                catch (System.InvalidProgramException) { return; }
                throw new System.Exception("A reference to a retiring frame escaped.");
            }
#endif
        }
    }
}
